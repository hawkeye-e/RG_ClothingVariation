

namespace RGClothingVariation
{
    //All internal values goes into here
    internal class Constant
    {
        internal const int DataStructureVersion = 1;

        internal const int ClothesPartCount = 8;
        internal const int ExtraFieldPartNumber = 8;

        internal static class ClothesPartObjectName
        {
            internal const string Top = "ct_clothesTop";
            internal const string Bottom = "ct_clothesBot";
            internal const string InnerTop = "ct_inner_t";
            internal const string InnerBottom = "ct_inner_b";
            internal const string Gloves = "ct_gloves";
            internal const string PantyHose = "ct_panst";
            internal const string Socks = "ct_socks";
            internal const string Shoes = "ct_shoes";
        }

        internal static class ClothesPart
        {
            internal const int Top = 0;
            internal const int Bottom = 1;
            internal const int InnerTop = 2;
            internal const int InnerBottom = 3;
            internal const int Gloves = 4;
            internal const int PantyHose = 5;
            internal const int Socks = 6;
            internal const int Shoes = 7;
        }

        internal static class ClothesPartName
        {
            internal const string Top = "トップス";
            internal const string Bottom = "ボトムス";
            internal const string InnerTop = "インナー上";
            internal const string InnerBottom = "インナー下";
            internal const string Gloves = "手袋";
            internal const string Pantyhose = "パンスト";
            internal const string Socks = "靴下";
            internal const string Shoes = "靴";
        }

        internal static class ClothesStateObjectName
        {
            internal const string TopFull = "n_top_a";
            internal const string TopHalf = "n_top_b";
            internal const string BottomFull = "n_bot_a";
            internal const string BottomHalf = "n_bot_b";
        }

        internal static class GeneralClothesStates
        {
            internal const int Full = 0;
            internal const int Half = 1;
            internal const int Nude = 2;
        }

        internal static class TwoStateClothesStates
        {
            internal const int Full = 0;
            internal const int Nude = 1;
        }

        internal static class CategoryBasedClothesStates
        {
            internal const int Default = 0;
            internal const int UnderwearOnly = 1;
            internal const int Nude = 2;
        }

        internal static class UserSelectedClothesStates
        {
            internal const int Default = 0;
            internal const int FullWear = 1;
            internal const int UnderwearOnly = 2;
            internal const int Nude = 3;
        }

        internal static class OverallClothesStateValue
        {
            internal readonly static int[] Default = { 0, 0, 0, 0, 0, 0, 0, 0 };
            internal readonly static int[] FullWear = { 0, 0, 0, 0, 0, 0, 0, 0 };
            internal readonly static int[] UnderwearOnly = { 2, 2, 0, 0, 0, 0, 0, 2 };
            internal readonly static int[] Nude = { 2, 2, 2, 2, 2, 2, 2, 2 };
        }

        internal static class ClothesCategoryToggleName
        {
            internal const string Top = "tglTop";
            internal const string Bottom = "tglBottom";
            internal const string InnerTop = "tglInnerTop";
            internal const string InnerBottom = "tglInnerBottom";
            internal const string Gloves = "tglGloves";
            internal const string PantyHose = "tglPantyhose";
            internal const string Socks = "tglSocks";
            internal const string Shoes = "tglShoes";
        }

    }
}
