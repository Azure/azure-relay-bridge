// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace HybridConnectionManagerIbizaUi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    public class IbizaStyleTableItem : Panel
    {
        List<Label> labels;

        bool mouseOver;

        bool selected;

        public IbizaStyleTableItem(Panel parent)
        {
            Fields = new Dictionary<string, TableField>();

            this.Size = new Size(parent.Width, 28);

            DoubleBuffered = true;
            ImageSize = new Size(25, 25);
            Dock = DockStyle.Top;
            TextFont = new Font(Font.FontFamily, 10, FontStyle.Regular);
            labels = new List<Label>();
        }

        public delegate void ClickEvent(EventArgs e);

        public delegate void SelectedEventHandler(object sender, EventArgs e);

        public event ClickEvent OnClicked;

        public event SelectedEventHandler OnSelected;

        public event SelectedEventHandler OnUnselected;

        public bool Disabled { get; set; }

        public bool Hoverable { get; set; }

        public Image Image { get; set; }

        public Size ImageSize { get; set; }

        public int Index { get; set; }

        public bool Selectable { get; set; }

        public bool Selected
        {
            get
            {
                return selected;
            }

            set
            {
                if (!selected && value)
                {
                    selected = value;
                    if (OnSelected != null)
                    {
                        OnSelected(this, EventArgs.Empty);
                    }
                }

                if (selected && !value)
                {
                    selected = value;
                    if (OnUnselected != null)
                    {
                        OnUnselected(this, EventArgs.Empty);
                    }
                }

                Invalidate();
            }
        }

        public Font TextFont { get; set; }

        protected TableColumn[] Columns { get; set; }

        protected Dictionary<string, TableField> Fields { get; }

        public virtual void Initialize(TableColumn[] columns)
        {
            this.Columns = columns;

            // Remove all labels
            foreach (var label in labels)
            {
                this.Controls.Remove(label);
            }

            labels.Clear();

            foreach (var column in columns)
            {
                // If we have a field for this column add it
                TableField field;
                if (Fields.TryGetValue(column.FieldName, out field))
                {
                    AddLabel(column.PositionPercent, column.PositionPixels, 0.2f, field.Text, field.EnabledColor,
                        field.DisabledColor);
                }
            }
        }

        public void OnMouseEnter(object sender, EventArgs e)
        {
            this.OnMouseEnter(e);
        }

        public void OnMouseLeave(object sender, EventArgs e)
        {
            this.OnMouseLeave(e);
        }

        protected void AddLabel(float horizontalPercent, int horizontalPixels, float verticalPercent, string text, Color enabledColor,
            Color disabledColor)
        {
            Label label = new Label();
            if (horizontalPixels != 0)
            {
                label.Location = new Point(horizontalPixels, (int)(this.Size.Height * verticalPercent));
            }
            else
            {
                label.Location = new Point((int)(this.Size.Width * horizontalPercent), (int)(this.Size.Height * verticalPercent));
            }

            label.Text = text;
            label.Font = TextFont;
            label.AutoSize = true;
            if (Disabled)
            {
                label.ForeColor = disabledColor;
            }
            else
            {
                label.ForeColor = enabledColor;
            }

            label.BackColor = Color.Transparent;

            label.MouseEnter += OnMouseEnter;
            label.MouseLeave += OnMouseLeave;
            label.MouseClick += OnMouseClick;

            this.Controls.Add(label);
            labels.Add(label);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (mouseOver && Selectable && !Selected && !Disabled && Hoverable)
            {
                Selected = true;
            }
            else if (Selected && !Disabled)
            {
                Selected = false;
            }

            if (OnClicked != null)
            {
                OnClicked(e);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            mouseOver = true;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseOver = false;
            base.OnMouseLeave(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Pen silverPen = new Pen(Color.FromArgb(255, 166, 166, 166), 2);

            Color hoverColor = Color.FromArgb(255, 217, 245, 253);
            Color selectedColor = Color.FromArgb(255, 178, 231, 250);

            // If an object is already selected, we will render it even if it's disabled.
            if ((!Disabled || Selected) &&
                Hoverable)
            {
                Rectangle actualRect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width,
                    ClientRectangle.Height - 2);
                using (
                    System.Drawing.Drawing2D.LinearGradientBrush brush =
                        new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, hoverColor, hoverColor, 90))
                using (
                    System.Drawing.Drawing2D.LinearGradientBrush selectedBrush =
                        new System.Drawing.Drawing2D.LinearGradientBrush(ClientRectangle, selectedColor, selectedColor,
                            90))
                {
                    if (Selected)
                    {
                        e.Graphics.FillRectangle(selectedBrush, actualRect);
                    }
                    else if (mouseOver)
                    {
                        e.Graphics.FillRectangle(brush, actualRect);
                    }
                }
            }

            e.Graphics.DrawLine(silverPen, new Point(ClientRectangle.Left, ClientRectangle.Bottom - 1),
                new Point(ClientRectangle.Right, ClientRectangle.Bottom - 1));
            base.OnPaint(e);
        }

        void OnMouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(e);
        }
    }
}