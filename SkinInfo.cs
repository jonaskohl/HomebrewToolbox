namespace WiiBrewToolbox
{
    public class SkinInfo
    {
        public string Filename { get; set; }
        public string DisplayName { get; set; }
        internal SkinInfoMeta Meta { get; set; }
    }

    internal enum SkinInfoMeta
    {
        None,
        NoAction,
        InternalSkin,
        GetSkins
    }
}
