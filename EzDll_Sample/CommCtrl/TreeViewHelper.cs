using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MFC.CommCtrl
{
    public class TreeViewHelper
    {
        public object Value
        {
            get { return mObj; }
        }

        public TreeViewItem ParentTree
        {
            get { return mParentTreeViewItem; }
        }

        private object mObj;
        private TreeViewItem mParentTreeViewItem;
        public TreeViewHelper(object obj, TreeViewItem parentTreeViewItem)
        {
            mObj = obj;
            mParentTreeViewItem = parentTreeViewItem;
        }

        public override string ToString()
        {
            return mObj.ToString();
        }                
    }
}
