// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell.Interop;

    // <summary>
    //     Implements a safer version of IVsObjectList for types that do no implement IVsCoTaskMemFreeMyStrings. Types that do not implement IVsCoTaskMemFreeMyStrings will
    //     cause an exception when marshalling strings from native code to the CLR, because the native code will pass its own reference to the string instead of a
    //     reference to a copy of the string thus causing bad things to happen when the CLR marshaller frees the memory for the native string after it converts the value
    //     to a CLR string. Therefore this interface changes the PInvoke for GetText() to return an out param of type IntPtr instead of string, which avoids the issue
    //     of the CLR marshaller causing memory exceptions since IntPtr's (unlike strings) aren't freed automatically.
    //     This however makes things more complicated for the callers who choose to use this interface (ISafeVsObjectList). They will now have to manually check their
    //     type to see if it implements IVsCoTaskMemFreeMyStrings, and if it does the caller will be responsible for freeing the memory at the returned IntPtr as
    //     implementers of IVsCoTaskMemFreeMyStrings will have passed a copy of their string and are no longer responsible for its memory management once it reaches
    //     the marshaller.
    // </summary>
    [ComImport]
    [InterfaceType((short)1)]
    [Guid("E37F46C4-C627-4D88-A091-2992EE33B51D")]
    internal interface ISafeVsObjectList2
    {
        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetFlags([ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREEFLAGS")] out uint pFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetItemCount([ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pCount);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetExpandedList(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfCanRecurse,
            [MarshalAs(UnmanagedType.Interface)] out IVsLiteTreeList pptlNode);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int LocateExpandedList(
            [In] [MarshalAs(UnmanagedType.Interface)] IVsLiteTreeList ExpandedList,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint iIndex);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int OnClose(
            [Out] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREECLOSEACTIONS")] [MarshalAs(UnmanagedType.LPArray)] VSTREECLOSEACTIONS[] ptca);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetText(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREETEXTOPTIONS")] VSTREETEXTOPTIONS tto, out IntPtr ppszText);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetTipText(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREETOOLTIPTYPE")] VSTREETOOLTIPTYPE eTipType,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppszText);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetExpandable(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfExpandable);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetDisplayData(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [Out] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREEDISPLAYDATA")] [MarshalAs(UnmanagedType.LPArray)] VSTREEDISPLAYDATA[] pData);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int UpdateCounter(
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pCurUpdate,
            [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREEITEMCHANGESMASK")] out uint pgrfChanges);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetListChanges(
            [In] [Out] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] ref uint pcChanges,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREELISTITEMCHANGE")] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] VSTREELISTITEMCHANGE[] prgListChanges);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int ToggleState(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSTREESTATECHANGEREFRESH")] out uint ptscr);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetCapabilities2([ComAliasName("Microsoft.VisualStudio.Shell.Interop.LIB_LISTCAPABILITIES2")] out uint pgrfCapabilities);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetList2(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.LIB_LISTTYPE2")] uint ListType,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.LIB_LISTFLAGS")] uint flags,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBSEARCHCRITERIA2")] [MarshalAs(UnmanagedType.LPArray)] VSOBSEARCHCRITERIA2[] pobSrch, [MarshalAs(UnmanagedType.Interface)] out IVsObjectList2 ppIVsObjectList2);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetCategoryField2(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.LIB_CATEGORY2")] int Category,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] out uint pfCatField);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetExpandable3(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.LIB_LISTTYPE2")] uint ListTypeExcluded,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfExpandable);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetNavigationInfo2(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [Out] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBNAVIGATIONINFO3")] [MarshalAs(UnmanagedType.LPArray)] VSOBNAVIGATIONINFO3[] pobNav);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int LocateNavigationInfo2(
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBNAVIGATIONINFO3")] [MarshalAs(UnmanagedType.LPArray)] VSOBNAVIGATIONINFO3[] pobNav,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBNAVNAMEINFONODE2")] [MarshalAs(UnmanagedType.LPArray)] VSOBNAVNAMEINFONODE2[] pobName, [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] int fDontUpdate,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfMatchedName,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pIndex);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetBrowseObject(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [MarshalAs(UnmanagedType.IDispatch)] out object ppdispBrowseObj);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetUserContext(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunkUserCtx);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int ShowHelp([In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetSourceContext(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index, [In] IntPtr pszFilename,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pulLineNum);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CountSourceItems(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [MarshalAs(UnmanagedType.Interface)] out IVsHierarchy ppHier,
            [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSITEMID")] out uint pItemid,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pcItems);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetMultipleSourceItems(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSGSIFLAGS")] uint grfGSI,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint cItems,
            [Out] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSITEMSELECTION")] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] VSITEMSELECTION[] rgItemSel);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CanGoToSource(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE")] VSOBJGOTOSRCTYPE SrcType,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfOK);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GoToSource(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJGOTOSRCTYPE")] VSOBJGOTOSRCTYPE SrcType);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetContextMenu(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index, out Guid pclsidActive,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")] out int pnMenuId,
            [MarshalAs(UnmanagedType.Interface)] out IOleCommandTarget ppCmdTrgtActive);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int QueryDragDrop(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [MarshalAs(UnmanagedType.Interface)] IDataObject pDataObject,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint grfKeyState,
            [In] [Out] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] ref uint pdwEffect);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int DoDragDrop(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [MarshalAs(UnmanagedType.Interface)] IDataObject pDataObject,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] uint grfKeyState,
            [In] [Out] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.DWORD")] ref uint pdwEffect);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CanRename(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR")] [MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfOK);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int DoRename(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.LPCOLESTR")] [MarshalAs(UnmanagedType.LPWStr)] string pszNewName,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJOPFLAGS")] uint grfFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int CanDelete(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.BOOL")] out int pfOK);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int DoDelete(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJOPFLAGS")] uint grfFlags);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int FillDescription(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJDESCOPTIONS")] uint grfOptions,
            [In] [MarshalAs(UnmanagedType.Interface)] IVsObjectBrowserDescription2 pobDesc);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int FillDescription2(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJDESCOPTIONS")] uint grfOptions,
            [In] [MarshalAs(UnmanagedType.Interface)] IVsObjectBrowserDescription3 pobDesc);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int EnumClipboardFormats(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJCFFLAGS")] uint grfFlags,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint celt,
            [Out] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJCLIPFORMAT")] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] VSOBJCLIPFORMAT[] rgcfFormats,
            [Out] [Optional] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] [MarshalAs(UnmanagedType.LPArray)] uint[] pcActual);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetClipboardFormat(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJCFFLAGS")] uint grfFlags,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.FORMATETC")] [MarshalAs(UnmanagedType.LPArray)] FORMATETC[] pFormatetc,
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.STGMEDIUM")] [MarshalAs(UnmanagedType.LPArray)] STGMEDIUM[] pMedium);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetExtendedClipboardVariant(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJCFFLAGS")] uint grfFlags,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJCLIPFORMAT")] [MarshalAs(UnmanagedType.LPArray)] VSOBJCLIPFORMAT[]
                pcfFormat, [MarshalAs(UnmanagedType.Struct)] out object pvarFormat);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetProperty(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [In] [ComAliasName("Microsoft.VisualStudio.Shell.Interop.VSOBJLISTELEMPROPID")] int propid,
            [MarshalAs(UnmanagedType.Struct)] out object pvar);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetNavInfo(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [MarshalAs(UnmanagedType.Interface)] out IVsNavInfo ppNavInfo);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int GetNavInfoNode(
            [In] [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] uint index,
            [MarshalAs(UnmanagedType.Interface)] out IVsNavInfoNode ppNavInfoNode);

        [PreserveSig]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        int LocateNavInfoNode(
            [In] [MarshalAs(UnmanagedType.Interface)] IVsNavInfoNode pNavInfoNode,
            [ComAliasName("Microsoft.VisualStudio.OLE.Interop.ULONG")] out uint pulIndex);
    }
}
