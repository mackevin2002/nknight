﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using nKnight.RBAC.SecurityLayer;

namespace nKnight.RBACControl
{
    public delegate void OnRBACGridError(GridControlEventArgs e);
    [DefaultProperty("GroupUniqueID")]
    public partial class nKnightGridView : DataGridView
    {

        public nKnightGridView()
        {
            InitializeComponent();
        }
        [Category("Action")]
        [Description("Fires when Error occures")]
        public event OnRBACGridError oError;
        void OnErrorRaised(GridControlEventArgs e)
        {
            if (oError != null)
            {
                oError(e);
            }
        }

        private string uniqueid;
        /// <summary>
        /// This is group unique id.
        /// </summary>
        public string GroupUniqueID
        {
            get
            {
                return uniqueid;
            }
            set
            {
                //if (IsDesignerHosted)
                //{
                uniqueid = value;
                Invalidate();
                //}
            }
        }

        /// <summary>
        /// Checking for property is in design mode or not
        /// </summary>
        private bool IsDesignerHosted
        {
            get
            {
                return IsControlDesignerHosted(this);
            }
        }

        private bool IsControlDesignerHosted(Control ctrl)
        {
            if (ctrl != null)
            {
                if (ctrl.Site != null)
                {
                    if (ctrl.Site.DesignMode == true)
                        return true;
                    else
                    {
                        if (IsControlDesignerHosted(ctrl.Parent))
                            return true;
                        else
                            return false;
                    }
                }
                else
                {
                    if (IsControlDesignerHosted(ctrl.Parent))
                        return true;
                    else
                        return false;
                }
            }

            else
                return false;
        }

        protected override void  OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            //MessageBox.Show(GroupUniqueID);
            string message = CheckSecurityPermission(GroupUniqueID);
            if (message == string.Empty) { base.OnCellBeginEdit(e); }
            else
            {
                e.Cancel = true;
                GridControlEventArgs cnt = new GridControlEventArgs();
                cnt.ErrorMessage = message;
                if (oError != null) { this.oError(cnt); }
            }
        }

        /// <summary>
        /// When control will be dropped into form then this will generate a unique GUID
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentChanged(EventArgs e)
        {
            if (IsDesignerHosted)
            {
                if (string.IsNullOrEmpty(uniqueid))
                {
                    uniqueid = System.Guid.NewGuid().ToString();
                }
            }
            base.OnParentChanged(e);
        }

        /// <summary>
        /// checks whether the user has the permission or not; this does not walk the stack
        /// as the standard .NET security classes; so it is not complete secure according to 
        /// the .NET framework, but it is an easy way how to check application specific 
        /// permissions
        /// </summary>
        /// <param name="Permission">the permission to check for</param>
        /// <returns>returns true if the user has the permission otherwise false</returns>
        private string CheckSecurityPermission(string pUniqueId)
        {
            string message = string.Empty;

            if (SecurityPrincipal.IsRBACInitialized)
            {
                if (SecurityPrincipal.IsRBACAuthenticated)
                {
                    if (!SecurityPrincipal.HasPermission(pUniqueId))
                    {
                        message = "Access denied";
                    }
                }
                else { message = "RBAC System is not authenticated"; }
            }
            else { message = "RBAC System is not initialized"; }
            return message;
        }
    }
    public class GridControlEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; }
    }
}
