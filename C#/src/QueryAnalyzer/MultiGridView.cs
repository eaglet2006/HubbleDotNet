using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace QueryAnalyzer
{
    class MultiGridView
    {
        Panel _Panel;

        List<DataGridView> _GridViewList;

        internal IList<DataGridView> GridViewList
        {
            get
            {
                return _GridViewList;
            }
        }

        internal MultiGridView(Panel panel, int gridviewCount)
        {
            _Panel = panel;

            _Panel.Controls.Clear();


            if (gridviewCount <= 0)
            {
                return;
            }


            _GridViewList = new List<DataGridView>();

            if (gridviewCount == 0)
            {
                DataGridView dgv = new DataGridView();
                dgv.Dock = DockStyle.Fill;
                dgv.Visible = true;
                _Panel.Controls.Add(dgv);
                _GridViewList.Add(dgv);
                return;
            }

            Panel lastPanel = _Panel;

            for (int i = 0; i < gridviewCount - 1; i++)
            {
                SplitContainer splitContainer = new SplitContainer();

                splitContainer.Dock = DockStyle.Fill;
                splitContainer.Panel1MinSize = 0;
                splitContainer.Orientation = Orientation.Horizontal;


                DataGridView dgv = new DataGridView();
                dgv.TabIndex = _Panel.Controls.Count;

                dgv.Dock = DockStyle.Fill;

                dgv.Visible = true;

                splitContainer.Panel1.Controls.Add(dgv);
                _GridViewList.Add(dgv);

                lastPanel.Controls.Add(splitContainer);

                splitContainer.SplitterDistance = _Panel.Height / gridviewCount;

                lastPanel = splitContainer.Panel2;
            }


            DataGridView lastdgv = new DataGridView();
            lastdgv.Dock = DockStyle.Fill;
            lastdgv.Visible = true;
            lastPanel.Controls.Add(lastdgv);
            _GridViewList.Add(lastdgv);

        }


    }
}
