using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chess
{
    unsafe public static class Memory
    {
        private const nuint DefaultAlignment = 64;
        private const nuint SmallPageSize = 4096;
        private const nuint LinuxLargePageSize = 2 * 1024 * 1024;

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint MEM_LARGE_PAGES = 0x20000000;
        private const uint PAGE_READWRITE = 0x04;

        public static T* Allocate_Array<T>(int count) where T : unmanaged
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            nuint bytes = (nuint) sizeof(T) * (nuint) count;
            T*    ptr   = (T*) Std_Aligned_Alloc(DefaultAlignment, bytes);
            Clear(ptr, bytes);
            return ptr;
        }

        public static void Free<T>(T* ptr) where T : unmanaged {
            Std_Aligned_Free(ptr);
        }

        public static void* Std_Aligned_Alloc(nuint alignment, nuint size) {
            if (alignment == 0 || (alignment & (alignment - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(alignment));

            if (size == 0)
                return null;

            void* memory = NativeMemory.AlignedAlloc(size, alignment);
            if (memory == null)
                throw new OutOfMemoryException();

            return memory;
        }

        public static void Std_Aligned_Free(void* ptr) {
            if (ptr != null)
                NativeMemory.AlignedFree(ptr);
        }

        public static void* Aligned_Large_Pages_Alloc(nuint allocSize) {
            if (allocSize == 0)
                return null;

            if (OperatingSystem.IsWindows())
            {
                void* mem = Aligned_Large_Pages_Alloc_Windows(allocSize);

                if (mem != null)
                    return mem;

                mem = VirtualAlloc(null, allocSize, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
                if (mem == null)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return mem;
            }

            void* portableMemory = Aligned_Large_Pages_Alloc_Portable(allocSize);
            Clear(portableMemory, allocSize);
            return portableMemory;
        }

        public static void Aligned_Large_Pages_Free(void* mem) {
            if (mem == null)
                return;

            if (OperatingSystem.IsWindows())
            {
                if (!VirtualFree(mem, 0, MEM_RELEASE))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return;
            }

            Std_Aligned_Free(mem);
        }

        public static bool Has_Large_Pages() {
            if (OperatingSystem.IsWindows())
            {
                nuint largePageSize = GetLargePageMinimum();
                if (largePageSize == 0)
                    return false;

                void* mem = Aligned_Large_Pages_Alloc_Windows(largePageSize);
                if (mem == null)
                    return false;

                Aligned_Large_Pages_Free(mem);
                return true;
            }

            return OperatingSystem.IsLinux();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Load_As<T>(byte* buffer) where T : unmanaged {
            return Unsafe.ReadUnaligned<T>(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Clear(void* ptr, nuint bytes) {
            if (ptr == null || bytes == 0)
                return;

            checked
            {
                new Span<byte>(ptr, (int) bytes).Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint Ceil_To_Multiple(nuint n, nuint baseValue) {
            return (n + baseValue - 1) / baseValue * baseValue;
        }

        private static void* Aligned_Large_Pages_Alloc_Portable(nuint allocSize) {
            nuint alignment = OperatingSystem.IsLinux() ? LinuxLargePageSize : SmallPageSize;
            nuint size      = Ceil_To_Multiple(allocSize, alignment);
            return Std_Aligned_Alloc(alignment, size);
        }

        private static void* Aligned_Large_Pages_Alloc_Windows(nuint allocSize) {
            nuint largePageSize = GetLargePageMinimum();
            if (largePageSize == 0)
                return null;

            nuint size = Ceil_To_Multiple(allocSize, largePageSize);
            return VirtualAlloc(null, size, MEM_RESERVE | MEM_COMMIT | MEM_LARGE_PAGES, PAGE_READWRITE);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void* VirtualAlloc(void* lpAddress,
                                                 nuint dwSize,
                                                 uint flAllocationType,
                                                 uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualFree(void* lpAddress, nuint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern nuint GetLargePageMinimum();
    }
}
