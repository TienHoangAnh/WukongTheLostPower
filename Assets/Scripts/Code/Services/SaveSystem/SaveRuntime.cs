public static class SaveRuntime
{
    public static SaveSlotDTO Current { get; set; } = new SaveSlotDTO();

    public static void Apply(SaveSlotDTO dto)
    {
        if (dto == null) return;
        Current = dto;
    }
}
