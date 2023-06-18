namespace ChooChoo
{
  internal readonly struct ImportableGood
  {
    public bool IsImportable { get; }

    public bool HasCapacity { get; }

    public TrainDistributableGood TrainDistributableGood { get; }

    private ImportableGood(
      bool isImportable,
      bool hasCapacity,
      TrainDistributableGood trainDistributableGood)
    {
      IsImportable = isImportable;
      HasCapacity = hasCapacity;
      TrainDistributableGood = trainDistributableGood;
    }

    public static ImportableGood CreateImportableWithCapacity(TrainDistributableGood trainDistributableGood) => new(true, true, trainDistributableGood);

    public static ImportableGood CreateNonImportable() => new(false, false, new TrainDistributableGood());

    public static ImportableGood CreateNonImportableWithCapacity() => new(false, true, new TrainDistributableGood());
  }
}
