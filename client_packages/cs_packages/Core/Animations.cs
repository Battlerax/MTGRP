using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using RAGE;
using RAGE.Game;
using RAGE.NUI;

namespace cs_packages.Core
{
    public class Animations : Events.Script
    {
        private bool _isInAnim = false;
        private SizeF _rect;

        public Animations()
        {
            _rect = RAGE.NUI.UIMenu.GetScreenResolutionMaintainRatio();
            Events.Tick += Tick;
            Events.Add("setPlayerIntoAnim", (args) => { _isInAnim = true; });
        }

        private void Tick(List<Events.TickNametagData> nametags)
        {
            if (Input.IsDown(32) && _isInAnim) //Space
            {
                _isInAnim = false;
                RAGE.Elements.Player.LocalPlayer.Call("stopPlayerAnims");
            }

            if (_isInAnim)
            {
                RAGE.NUI.UIResText.Draw("~o~Press SPACE to stop the animation", (int) (_rect.Width / 2),
                    (int) (_rect.Height - 100), Font.Monospace, 0.75f, Color.White, UIResText.Alignment.Left, false,
                    true, 0);
                if(RAGE.Elements.Player.LocalPlayer.IsInWater()) { /* checking if player is in water */
                    _isInAnim = false;
                    RAGE.Elements.Player.LocalPlayer.Call("stopPlayerAnims");
                }
            }
        }
    }
}
