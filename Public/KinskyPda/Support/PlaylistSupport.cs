using System;
using System.Collections.Generic;
using System.Text;

namespace KinskyPda
{
    public class PlaylistSupport : Linn.Kinsky.PlaySupport
    {
        public PlaylistSupport(ImageButton aPlayNowButton, ImageButton aPlayLaterButton)
        {
            aPlayNowButton.Click += aPlayNowButton_Click;
            aPlayLaterButton.Click += aPlayLaterButton_Click;        
        }

        private void aPlayLaterButton_Click(object sender, EventArgs e)
        {
            base.FireEventPlayLaterRequest();
        }

        private void aPlayNowButton_Click(object sender, EventArgs e)
        {
            base.FireEventPlayNowRequest();
        }
    }
}
