namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the armTemplateArmTemplateParameters table in the database.
    /// </summary>
    public partial class ArmTemplateArmTemplateParameter
    {
        public long ArmTemplateId { get; set; }
        public ArmTemplate ArmTemplate { get; set; }

        public long ArmTemplateParameterId { get; set; }
        public ArmTemplateParameter ArmTemplateParameter { get; set; }
    }
}
