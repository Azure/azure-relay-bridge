// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class CustomButton : Control
    {
        Label buttonLabel;

        State currentState;

        public delegate void ButtonEvent(object sender, EventArgs e);

        public event ButtonEvent OnClicked;

        enum State
        {
            Deactivated,

            Unselected,

            Hovering,

            ClickedHovering,

            ClickedNotHovering
        }

        public string ButtonText { get; set; }

        public Image ClickImage { get; set; }

        public Image DeactivatedImage { get; set; }

        public Color DisabledTextColor { get; set; }

        public Color EnabledTextColor { get; set; }

        public Image HoverImage { get; set; }

        public Font TextFont { get; set; }

        public Point TextPosition { get; set; }

        public Image UnselectedImage { get; set; }

        public void Activate()
        {
            if (currentState == State.Deactivated)
            {
                currentState = State.Unselected;

                if (buttonLabel != null)
                {
                    buttonLabel.ForeColor = EnabledTextColor;
                }

                this.Invalidate();
            }
        }

        public void Deactivate()
        {
            currentState = State.Deactivated;
            if (buttonLabel != null)
            {
                buttonLabel.ForeColor = DisabledTextColor;
            }

            this.Invalidate();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.DoubleBuffered = true;
            currentState = State.Unselected;
            this.BringToFront();

            if (!string.IsNullOrEmpty(ButtonText))
            {
                buttonLabel = new Label();
                buttonLabel.Text = ButtonText;
                buttonLabel.BackColor = Color.Transparent;
                buttonLabel.Font = TextFont;
                buttonLabel.ForeColor = EnabledTextColor;
                buttonLabel.AutoSize = true;
                buttonLabel.TextAlign = ContentAlignment.MiddleCenter;
                buttonLabel.Location = new Point((this.Width / 2) - (buttonLabel.PreferredWidth / 2),
                    (this.Height / 2) - (buttonLabel.PreferredHeight / 2));
                buttonLabel.BringToFront();
                buttonLabel.MouseEnter += OnMouseEnter;
                buttonLabel.MouseLeave += OnMouseLeave;
                buttonLabel.MouseDown += OnMouseDown;
                buttonLabel.MouseUp += OnMouseUp;
                this.Controls.Add(buttonLabel);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (currentState == State.Deactivated)
            {
                return;
            }

            this.Invalidate();

            if (currentState == State.Hovering)
            {
                currentState = State.ClickedHovering;
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (currentState == State.Deactivated)
            {
                return;
            }

            if (currentState == State.Unselected)
            {
                currentState = State.Hovering;
            }
            else if (currentState == State.ClickedNotHovering)
            {
                currentState = State.ClickedHovering;
            }

            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (currentState == State.Deactivated)
            {
                return;
            }

            if (currentState == State.Hovering)
            {
                currentState = State.Unselected;
            }

            if (currentState == State.ClickedHovering)
            {
                currentState = State.ClickedNotHovering;
            }

            this.Invalidate();
        }

        // When the mouse is released, reset the "pressed" flag 
        // and invalidate to redraw the button in the unpressed state. 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.Invalidate();

            if (currentState == State.Deactivated)
            {
                return;
            }

            if (ClientRectangle.Contains(PointToClient(Control.MousePosition)))
            {
                if (currentState == State.ClickedHovering)
                {
                    // This is a click.
                    currentState = State.Hovering;
                    if (OnClicked != null)
                    {
                        OnClicked(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                currentState = State.Unselected;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (currentState == State.Deactivated)
            {
                if (DeactivatedImage != null)
                {
                    e.Graphics.DrawImage(DeactivatedImage, 0, 0);
                }
            }
            else if (currentState == State.Unselected || currentState == State.ClickedNotHovering)
            {
                if (UnselectedImage != null)
                {
                    e.Graphics.DrawImage(UnselectedImage, 0, 0);
                }
            }
            else if (currentState == State.Hovering)
            {
                if (HoverImage != null)
                {
                    e.Graphics.DrawImage(HoverImage, 0, 0);
                }
                else
                {
                    e.Graphics.DrawImage(UnselectedImage, 0, 0);
                }
            }
            else if (currentState == State.ClickedHovering)
            {
                if (ClickImage != null)
                {
                    e.Graphics.DrawImage(ClickImage, 0, 0);
                }
                else
                {
                    e.Graphics.DrawImage(UnselectedImage, 0, 0);
                }
            }

            if (buttonLabel != null)
            {
                buttonLabel.BringToFront();
            }
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        void OnMouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        void OnMouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }

        void OnMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }
    }
}