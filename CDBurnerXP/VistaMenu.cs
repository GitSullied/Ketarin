﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;


//VistaMenu 1.1, created by Wyatt O'Day
//Visit: http://wyday.com/blog/2008/vista-menu-with-icons-in-c-sharp/

namespace CDBurnerXP.Controls
{
    //Properties for the MenuItem
    internal class Properties
    {
        public Image Image = null;
        public IntPtr renderBmpHbitmap = IntPtr.Zero;
    }

    [ProvideProperty("Image", typeof(MenuItem))]
    public partial class VistaMenu : System.ComponentModel.Component, IExtenderProvider, ISupportInitialize
    {
        private Container components = null;
        private Hashtable properties = new Hashtable();
        private Hashtable menuParents = new Hashtable();

        private bool formHasBeenIntialized = false;
        private bool isVistaOrLater = false;

        #region Imports

        public const int MIIM_BITMAP = 0x00000080;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetMenuItemInfo(HandleRef hMenu, int uItem, bool fByPosition, MENUITEMINFO_T_RW lpmii);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetMenuInfo(HandleRef hMenu, MENUINFO lpcmi);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        #endregion


        public VistaMenu()
        {
            isVistaOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major >= 6;

            InitializeComponent();
        }

        public VistaMenu(System.ComponentModel.IContainer container)
            : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //release all the HBitmap handles created
                foreach (DictionaryEntry de in properties)
                {
                    if (((Properties)de.Value).renderBmpHbitmap != IntPtr.Zero)
                        DeleteObject(((Properties)de.Value).renderBmpHbitmap);
                }


                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        bool IExtenderProvider.CanExtend(object o)
        {
            if (o is MenuItem || o is Form)
                return true;
            else
                return false;
        }

        private Properties EnsurePropertiesExists(MenuItem key)
        {
            Properties p = (Properties)properties[key];

            if (p == null)
            {
                p = new Properties();

                properties[key] = p;
            }

            return p;
        }


        #region MenuItem.Image


        [Description("The Image for the MenuItem")]
        [Category("Appearance")]
        public Image GetImage(MenuItem mnuItem)
        {
            return EnsurePropertiesExists(mnuItem).Image;
        }

        public void SetImage(MenuItem mnuItem, Image value)
        {
            Properties prop = EnsurePropertiesExists(mnuItem);
            
            prop.Image = value;


            if (!DesignMode && isVistaOrLater)
            {
                //Destroy old bitmap object
                if (prop.renderBmpHbitmap != IntPtr.Zero)
                {
                    DeleteObject(prop.renderBmpHbitmap);
                    prop.renderBmpHbitmap = IntPtr.Zero;
                }

                //if there's no Image, then just bail out
                if (value == null)
                    return;

                //convert to 32bppPArgb (the 'P' means The red, green, and blue components are premultiplied, according to the alpha component.)
                Bitmap renderBmp = new Bitmap(value.Width, value.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                Graphics g = Graphics.FromImage(renderBmp);

                g.DrawImage(value, 0, 0, value.Width, value.Height);
                g.Dispose();

                prop.renderBmpHbitmap = renderBmp.GetHbitmap(Color.FromArgb(0, 0, 0, 0));
                renderBmp.Dispose();

                if (formHasBeenIntialized)
                {
                    AddVistaMenuItem(mnuItem);
                }
            }


            //for every Pre-Vista Windows, add the parent of the menu item to the list of parents
            if (!DesignMode && !isVistaOrLater && formHasBeenIntialized)
            {
                AddPreVistaMenuItem(mnuItem);
            }
        }

        private bool ShouldSerializeImage(MenuItem mnuItem)
        {
            if (GetImage(mnuItem) != null)
                return true;
            else
                return false;
        }

        private void ResetImage(MenuItem mnuItem)
        {
            SetImage(mnuItem, null);
        }



        #endregion



        void ISupportInitialize.BeginInit()
        {
        }

        MENUINFO mnuInfo = new MENUINFO();

        void AddVistaMenuItem(MenuItem mnuItem)
        {
            //get the bitmap children of the parent
            List<MenuItem> mnuBitmapChildren = (List<MenuItem>)menuParents[mnuItem.Parent.Handle];


            if (mnuBitmapChildren == null)
            {
                if (mnuItem.Parent.GetType() == typeof(ContextMenu))
                    ((ContextMenu)mnuItem.Parent).Popup += new EventHandler(MenuItem_Popup);
                else
                    ((MenuItem)mnuItem.Parent).Popup += new EventHandler(MenuItem_Popup);

                //intialize all the topmost menus to be of type "MNS_CHECKORBMP" (for Vista classic theme)
                SetMenuInfo(new HandleRef(null, mnuItem.Parent.Handle), mnuInfo);


                mnuBitmapChildren = new List<MenuItem>();

                mnuBitmapChildren.Add(mnuItem);

                //set the new children list to the corresponding parent
                menuParents[mnuItem.Parent.Handle] = mnuBitmapChildren;
            }
            else
            {
                mnuBitmapChildren.Add(mnuItem);
            }
        }

        void AddPreVistaMenuItem(MenuItem mnuItem)
        {
            if (menuParents[mnuItem.Parent] == null)
            {
                if (mnuItem.Parent.GetType() == typeof(ContextMenu))
                    ((ContextMenu)mnuItem.Parent).Popup += new EventHandler(PreVistaMenuItem_Popup);
                else
                    ((MenuItem)mnuItem.Parent).Popup += new EventHandler(PreVistaMenuItem_Popup);


                menuParents[mnuItem.Parent] = true;

                if (formHasBeenIntialized)
                {
                    //add all the menu items with custom paint events
                    foreach (MenuItem menu in mnuItem.Parent.MenuItems)
                    {
                        menu.DrawItem += new DrawItemEventHandler(MenuItem_DrawItem);
                        menu.MeasureItem += new MeasureItemEventHandler(MenuItem_MeasureItem);
                        menu.OwnerDraw = true;
                    }
                }
            }
        }

        void ISupportInitialize.EndInit()
        {
            if (!DesignMode)
            {
                if (isVistaOrLater)
                {
                    foreach (DictionaryEntry de in properties)
                    {
                        AddVistaMenuItem((MenuItem)de.Key);
                    }
                }
                else // Pre-Vista menus
                {
                    foreach (DictionaryEntry de in properties)
                    {
                        AddPreVistaMenuItem((MenuItem)de.Key);
                    }

                    //add event handle for each menu item's measure & draw routines
                    foreach (DictionaryEntry parent in menuParents)
                    {
                        foreach (MenuItem mnuItem in ((Menu)parent.Key).MenuItems)
                        {
                            mnuItem.DrawItem += new DrawItemEventHandler(MenuItem_DrawItem);
                            mnuItem.MeasureItem += new MeasureItemEventHandler(MenuItem_MeasureItem);
                            mnuItem.OwnerDraw = true;
                        }
                    }
                }

                formHasBeenIntialized = true;
            }
        }


        void MenuItem_Popup(object sender, EventArgs e)
        {
            //get the parentHandle
            IntPtr parentHandle = ((Menu)sender).Handle;

            //get the list of children menuitems to "refresh"
            List<MenuItem> mnuBitmapChildren = (List<MenuItem>)menuParents[parentHandle];

            MENUITEMINFO_T_RW menuItemInfo = new MENUITEMINFO_T_RW();
            menuItemInfo.fMask = MIIM_BITMAP;

            foreach (MenuItem menuItem in mnuBitmapChildren)
            {
                //menuItem.
                menuItemInfo.hbmpItem = ((Properties)properties[menuItem]).renderBmpHbitmap;

                //refresh the menu item
                SetMenuItemInfo(new HandleRef(null, parentHandle),
                    (int)typeof(MenuItem).InvokeMember("MenuID", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, menuItem, null),
                    false,
                    menuItemInfo);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MENUITEMINFO_T_RW
    {
        public int cbSize = Marshal.SizeOf(typeof(MENUITEMINFO_T_RW));
        public int fMask;
        public int fType;
        public int fState;
        public int wID;
        public IntPtr hSubMenu = IntPtr.Zero;
        public IntPtr hbmpChecked = IntPtr.Zero;
        public IntPtr hbmpUnchecked = IntPtr.Zero;
        public IntPtr dwItemData = IntPtr.Zero;
        public IntPtr dwTypeData = IntPtr.Zero;
        public int cch;
        public IntPtr hbmpItem = IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MENUINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MENUINFO));
        public int fMask = 0x00000010; //MIM_STYLE;
        public int dwStyle = 0x04000000; //MNS_CHECKORBMP;
        public uint cyMax;
        public IntPtr hbrBack = IntPtr.Zero;
        public int dwContextHelpID;
        public IntPtr dwMenuData = IntPtr.Zero;
    }
}