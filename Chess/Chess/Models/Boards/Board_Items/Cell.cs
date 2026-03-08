namespace Chess
{
    public abstract class Cell(Position position) : Item
    {
        public virtual uint Column { get; set; }
        public virtual Position Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }
    }
}
