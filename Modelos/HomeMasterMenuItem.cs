namespace TaxistasMaui.Modelos
{
    public class HomeMasterMenuItem
    {
        public HomeMasterMenuItem()
        {
            TargetType = typeof(HomeMasterMenuItem);
        }
        public int Id { get; set; }
        public string? Title { get; set; }
        public Type TargetType { get; set; }
    }
}
