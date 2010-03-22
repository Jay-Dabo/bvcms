﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml.Linq;
using System.Configuration;
using System.IO;
using System.Collections.Specialized;

namespace CmsCheckin
{
    public partial class ListClasses : UserControl
    {
        public ListClasses()
        {
            InitializeComponent();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = Program.Interval;
        }

        DateTime time;
        int FamilyId;
        int PeopleId;
        int? next, prev;
        List<Control> controls = new List<Control>();

        public void ShowResults(int pid, int page)
        {
            ClearControls();
            var x = this.GetDocument("Checkin/Classes/" + pid + Program.QueryString + "&page=" + page);

            time = DateTime.Now;

            var points = 14F;

            string Verdana = "Verdana";
            Font labfont;
            var g = this.CreateGraphics();
            FamilyId = x.Root.Attribute("fid").Value.ToInt();
            PeopleId = x.Root.Attribute("pid").Value.ToInt();
            next = x.Root.Attribute("next").Value.ToInt2();
            prev = x.Root.Attribute("prev").Value.ToInt2();
            pgdn.Visible = next.HasValue;
            pgup.Visible = prev.HasValue;

            if (x.Descendants("class").Count() == 0)
            {
                var lab = new Label();
                lab.Font = new Font(Verdana, points, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                lab.Location = new Point(15, 200);
                lab.AutoSize = true;
                lab.Text = "Sorry, no classes found";
                this.Controls.Add(lab);
                controls.Add(lab);
                GoBackButton.Text = "Go Back";
                return;
            }

            while (true)
            {
                var wid = 0;
                labfont = new Font(Verdana, points, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                foreach (var e in x.Descendants("class"))
                {
                    var s = e.Attribute("display").Value;
                    var size = g.MeasureString(s, labfont);
                    wid = Math.Max(wid, (int)size.Width);
                }
                if (wid > 1000)
                {
                    points -= 1F;
                    continue;
                }
                break;
            }
            var row = 0;
            foreach (var e in x.Descendants("class"))
            {
                var ab = new Button();
                controls.Add(ab);
                ab.BackColor = SystemColors.ControlLight;
                ab.Font = labfont;
                ab.Location = new Point(10, 100 + (row * 50));
                ab.Size = new Size(1000, 45);
                ab.TextAlign = ContentAlignment.MiddleLeft;
                ab.UseVisualStyleBackColor = false;
                var c = new ClassInfo
                {
                    oid = e.Attribute("orgid").Value.ToInt(),
                    pid = PeopleId,
                };
                ab.Tag = c;
                ab.Text = e.Attribute("display").Value;
                this.Controls.Add(ab);
                ab.Click += new EventHandler(ab_Click);
                ab.KeyPress += new KeyPressEventHandler(ResultKeyPress);
                row++;
            }
            timer1.Start();
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Program.ClearFields();
            this.GoHome("");
        }

        void ab_Click(object sender, EventArgs e)
        {
            var ab = sender as Button;
            var c = ab.Tag as ClassInfo;
            this.RecordAttend(c, true);
            timer1.Stop();
            this.Swap(Program.family);
            Program.family.ShowFamily(FamilyId, 1);
        }

        private void GoBack_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Swap(Program.family);
            Program.family.ShowFamily(FamilyId, 1);
        }
        private void ResultKeyPress(object sender, KeyPressEventArgs e)
        {
            timer1.Stop();
            timer1.Start();
            if (e.KeyChar == 27)
            {
                this.Swap(Program.family);
                Program.family.ShowFamily(FamilyId, 1);
            }
        }

        private void ClearControls()
        {
            foreach (var c in controls)
            {
                this.Controls.Remove(c);
                c.Dispose();
            }
            controls.Clear();
        }
        private void pgdn_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            ShowResults(PeopleId, next.Value);
        }

        private void pgup_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            ShowResults(PeopleId, prev.Value);
        }
    }
    public class ClassInfo
    {
        public int oid { get; set; }
        public int pid { get; set; }
        public string mv { get; set; }
    }
}
