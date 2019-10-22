namespace mtgvrp.player_manager
{
    public class Model
    {
        //Model Related 
        public int Gender { get; set; } //0 = Male, 1 = Female
        public int FatherId { get; set; }
        public int MotherId { get; set; }
        public float ParentLean { get; set; }

        public int HairStyle { get; set; }
        public int HairColor { get; set; }
        public int Blemishes { get; set; }
        public int FacialHair { get; set; }
        public int Eyebrows { get; set; }
        public int Ageing { get; set; }
        public int Makeup { get; set; }
        public int MakeupColor { get; set; }
        public int Blush { get; set; }
        public int BlushColor { get; set; }
        public int Lipstick { get; set; }
        public int LipstickColor { get; set; }
        public int Complexion { get; set; }
        public int SunDamage { get; set; }
        public int MolesFreckles { get; set; }

        public int PantsStyle { get; set; }
        public int PantsVar { get; set; }
        public int ShoeStyle { get; set; }
        public int ShoeVar { get; set; }
        public int AccessoryStyle { get; set; }
        public int AccessoryVar { get; set; }
        public int UndershirtStyle { get; set; }
        public int UndershirtVar { get; set; }
        public int TopStyle { get; set; }
        public int TopVar { get; set; }
        public int HatStyle { get; set; }
        public int HatVar { get; set; }
        public int GlassesStyle { get; set; }
        public int GlassesVar { get; set; }
        public int EarStyle { get; set; }
        public int EarVar { get; set; }

        public int TorsoStyle { get; set; }
        public int TorsoVar { get; set; }

        public void SetDefault()
        {
            HairStyle = 0;
            HairColor = 0;
            Blemishes = 255;
            FacialHair = 255;
            Eyebrows = 0;
            Ageing = 255;
            Makeup = 255;
            MakeupColor = 0;
            Blush = 255;
            BlushColor = 0;
            Lipstick = 255;
            LipstickColor = 0;
            Complexion = 255;
            SunDamage = 255;
            MolesFreckles = 255;

            if (Gender == 0)
            {
                PantsStyle = 0;
                PantsVar = 1;

                ShoeStyle = 0;
                ShoeVar = 11;

                AccessoryStyle = 0;
                AccessoryVar = 0;

                UndershirtStyle = 5;
                UndershirtVar = 1;

                TopStyle = 0;
                TopVar = 1;

                HatStyle = 8;
                HatVar = 1;

                GlassesStyle = 0;
                GlassesVar = 1;

                EarStyle = 255;
                EarVar = 0;
            }
            else
            {
                PantsStyle = 0;
                PantsVar = 1;

                ShoeStyle = 0;
                ShoeVar = 1;

                AccessoryStyle = 0;
                AccessoryVar = 1;

                UndershirtStyle = 3;
                UndershirtVar = 1;

                TopStyle = 0;
                TopVar = 1;

                HatStyle = 57;
                HatVar = 1;

                GlassesStyle = 5;
                GlassesVar = 1;

                EarStyle = 225;
                EarVar = 0;
            }
        }
    }
}
