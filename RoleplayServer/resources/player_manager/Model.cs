using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleplayServer
{
    public class Model
    {
        //Model Related 
        public int gender { get; set; } //0 = Male, 1 = Female
        public int father_id { get; set; }
        public int mother_id { get; set; }
        public float parent_lean { get; set; }

        public int hair_style { get; set; }
        public int hair_color { get; set; }
        public int blemishes { get; set; }
        public int facial_hair { get; set; }
        public int eyebrows { get; set; }
        public int ageing { get; set; }
        public int makeup { get; set; }
        public int makeup_color { get; set; }
        public int blush { get; set; }
        public int blush_color { get; set; }
        public int lipstick { get; set; }
        public int lipstick_color { get; set; }
        public int complexion { get; set; }
        public int sun_damage { get; set; }
        public int moles_freckles { get; set; }

        public int pants_style { get; set; }
        public int pants_var { get; set; }
        public int shoe_style { get; set; }
        public int shoe_var { get; set; }
        public int accessory_style { get; set; }
        public int accessory_var { get; set; }
        public int undershirt_style { get; set; }
        public int undershirt_var { get; set; }
        public int top_style { get; set; }
        public int top_var { get; set; }
        public int hat_style { get; set; }
        public int hat_var { get; set; }
        public int glasses_style { get; set; }
        public int glasses_var { get; set; }
        public int ear_style { get; set; }
        public int ear_var { get; set; }

        public Model()
        {
            father_id = 0;
            mother_id = 21;
            parent_lean = 10;
            gender = 0;

            hair_style = 0;
            hair_color = 0;
            blemishes = 255;
            facial_hair = 255;
            eyebrows = 0;
            ageing = 255;
            makeup = 255;
            makeup_color = 0;
            blush = 255;
            blush_color = 0;
            lipstick = 255;
            lipstick_color = 0;
            complexion = 255;
            sun_damage = 255;
            moles_freckles = 255;

            pants_style = 0;
            pants_var = 0;
            shoe_style = 0;
            shoe_var = 0;
            accessory_style = 0;
            accessory_var = 0;
            undershirt_style = 0;
            undershirt_var = 0;
            top_style = 0;
            top_var = 0;
            hat_style = -1;
            hat_var = 0;
            glasses_style = -1;
            glasses_var = 0;
            ear_style = -1;
            ear_var = 0;
        }

        public void setDefault()
        {
            hair_style = 0;
            hair_color = 0;
            blemishes = 255;
            facial_hair = 255;
            eyebrows = 0;
            ageing = 255;
            makeup = 255;
            makeup_color = 0;
            blush = 255;
            blush_color = 0;
            lipstick = 255;
            lipstick_color = 0;
            complexion = 255;
            sun_damage = 255;
            moles_freckles = 255;

            if (gender == 0)
            {
                pants_style = 0;
                pants_var = 1;

                shoe_style = 0;
                shoe_var = 11;

                accessory_style = 0;
                accessory_var = 0;

                undershirt_style = 5;
                undershirt_var = 1;

                top_style = 0;
                top_var = 1;

                hat_style = 8;
                hat_var = 1;

                glasses_style = 0;
                glasses_var = 1;

                ear_style = 255;
                ear_var = -1;
            }
            else
            {
                pants_style = 0;
                pants_var = 1;

                shoe_style = 0;
                shoe_var = 1;

                accessory_style = 0;
                accessory_var = 1;

                undershirt_style = 3;
                undershirt_var = 1;

                top_style = 0;
                top_var = 1;

                hat_style = 57;
                hat_var = 1;

                glasses_style = 5;
                glasses_var = 1;

                ear_style = 0;
                ear_var = -1;
            }
        }
    }
}
