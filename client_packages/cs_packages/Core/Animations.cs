using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using RAGE;

namespace cs_packages.Core
{
    public class Animations : Events.Script
    {
        private bool _isInAnim = false;
        

        public Animations()
        {
            //RAGE.Game.Graphics.resol
            Events.Tick += Tick;

        }

        private void Tick(List<Events.TickNametagData> nametags)
        {
            //RAGE.NUI.UIResText.Draw("~o~Press SPACE to stop the animation", new Point(), );
        }
    }
}
