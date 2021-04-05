using PortableDeviceApiLib;
using PortableDeviceTypesLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;                                                              // ListViewItem
using _tagpropertykey = PortableDeviceApiLib._tagpropertykey;
using IPortableDeviceKeyCollection = PortableDeviceApiLib.IPortableDeviceKeyCollection;
using IPortableDeviceValues = PortableDeviceApiLib.IPortableDeviceValues;

namespace PortableDevices {
    public class PortableDevice {
        #region Fields

        private bool _isConnected;
        private readonly PortableDeviceClass _device;

        #endregion

        #region ctor(s)

        public PortableDevice(string deviceId) {
            this._device = new PortableDeviceClass();
            this.DeviceId = deviceId;
        }

        #endregion

        #region Properties

        public string DeviceId { get; set; }

        public string FriendlyName {
            get {
                if ( !this._isConnected ) {
                    throw new InvalidOperationException("Not connected to device.");
                }

                // Retrieve the properties of the device
                IPortableDeviceContent content;
                IPortableDeviceProperties properties;
                this._device.Content(out content);
                content.Properties(out properties);

                // Retrieve the values for the properties
                IPortableDeviceValues propertyValues;
                properties.GetValues("DEVICE", null, out propertyValues);

                // Identify the property to retrieve
                _tagpropertykey property = new _tagpropertykey();
                property.fmtid = new Guid(0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC);
                property.pid = 12;

                // Retrieve the friendly name
                string propertyValue;
                propertyValues.GetStringValue(ref property, out propertyValue);

                return propertyValue;
            }
        }

        public string DeviceModel {
            get {
                if ( !this._isConnected ) {
                    throw new InvalidOperationException("Not connected to device.");
                }

                // Retrieve the properties of the device
                IPortableDeviceContent content;
                IPortableDeviceProperties properties;
                this._device.Content(out content);
                content.Properties(out properties);

                // Retrieve the values for the properties
                IPortableDeviceValues propertyValues;
                properties.GetValues("DEVICE", null, out propertyValues);

                // Identify the property to retrieve --> https://github.com/notpod/wpd-lib/issues/1 the last 2 answers
                _tagpropertykey property = new _tagpropertykey();
                property.fmtid = new Guid(0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B, 0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC);
                property.pid = 8;

                // Retrieve the device model wich represents a name as shown in explorer's "This Computer"
                string propertyValue;
                propertyValues.GetStringValue(ref property, out propertyValue);

                return propertyValue;
            }
        }

        internal PortableDeviceClass PortableDeviceClass {
            get {
                return this._device;
            }
        }

        #endregion

        #region Methods

        public void Connect() {
            if ( this._isConnected ) { return; }

            IPortableDeviceValues clientInfo = (IPortableDeviceValues)new PortableDeviceValuesClass();
            this._device.Open(this.DeviceId, clientInfo);
            this._isConnected = true;
        }

        public void Disconnect() {
            if ( !this._isConnected ) { return; }
            this._device.Close();
            this._isConnected = false;
        }

        // read an arbitrary targetFolder (only this one) from WPD and return its content as a List<string>
        public class wpdFileInfo {
            public long size;
            public string date;
            public string name;
            public string id;
            public wpdFileInfo(string name, string id, long size, string date) {
                this.name = name;
                this.id = id;
                this.size = size;
                this.date = date;
            }
        }
        public List<wpdFileInfo> GetFolderContentList(string targetFolder, ref string targetFolderID) {
            // it always starts from the device' root, ensures to be up to date 
            string[] arr = targetFolder.Split('\\');
            string rootFolder = arr[0];
            PortableDeviceFolder root = new PortableDeviceFolder("DEVICE", rootFolder);
            List<wpdFileInfo> retlist = new List<wpdFileInfo>();
            targetFolderID = "";
            IPortableDeviceContent content;
            this._device.Content(out content);
            EnumerateFolderContent(ref content, root, rootFolder, targetFolder, ref retlist, ref targetFolderID);
            return retlist;
        }
        private static void EnumerateFolderContent(ref IPortableDeviceContent content, PortableDeviceFolder parent, string currentFolder, string targetFolder, ref List<wpdFileInfo> retlist, ref string targetFolderID) {
            // save ID of the parent when target folder is reached
            if ( targetFolder == currentFolder ) {
                targetFolderID = parent.Id;
            }
            // Get the properties of the object
            IPortableDeviceProperties properties;
            content.Properties(out properties);
            // Enumerate the items contained by the current object
            IEnumPortableDeviceObjectIDs objectIds;
            content.EnumObjects(0, parent.Id, null, out objectIds);
            uint fetched = 0;
            do {
                string objectId;
                objectIds.Next(1, out objectId, ref fetched);
                if ( fetched > 0 ) {
                    PortableDeviceObject currentObject = WrapObject(properties, objectId);
                    // only collect objects within the targetFolder, ignore all other object 
                    if ( targetFolder == currentFolder ) {
                        // build item
                        string name = currentObject.Name;
                        if ( currentObject is PortableDeviceFolder ) {
                            // get file modified date
                            IPortableDeviceKeyCollection keys;
                            properties.GetSupportedProperties(currentObject.Id, out keys);
                            IPortableDeviceValues values;
                            properties.GetValues(currentObject.Id, keys, out values);
                            string objDateStr = "";
                            _tagpropertykey property = new _tagpropertykey();
                            property.fmtid = new Guid("EF6B490D-5CD8-437A-AFFC-DA8B60EE4A3C");
                            property.pid = 19;   //WPD_OBJECT_DATE_MODIFIED
                            try {
                                values.GetStringValue(property, out objDateStr);  // 2016/10/19:18:52:52.000
                            } catch {; }
                            retlist.Add(new wpdFileInfo("p:" + name, "wpd_" + currentObject.Id, 0, objDateStr));
                        }
                        if ( currentObject is PortableDeviceFile ) {

                            // get file properties
                            IPortableDeviceKeyCollection keys;
                            properties.GetSupportedProperties(currentObject.Id, out keys);
                            IPortableDeviceValues values;
                            properties.GetValues(currentObject.Id, keys, out values);

                            // get file size
                            long objSize;
                            _tagpropertykey property = new _tagpropertykey();
                            property.fmtid = new Guid("EF6B490D-5CD8-437A-AFFC-DA8B60EE4A3C");
                            property.pid = 11;   //WPD_OBJECT_SIZE;
                            values.GetSignedLargeIntegerValue(property, out objSize);

                            // get file modified date
                            string objDateStr;
                            property.pid = 19;   //WPD_OBJECT_DATE_MODIFIED
                            values.GetStringValue(property, out objDateStr);  // 2016/10/19:18:52:52.000
                            //DateTime objDateUTC = (DateTime.ParseExact(objDateStr, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal)).ToUniversalTime();  // 2016/10/19:18:52:52.000

                            // transfer file to return list
                            retlist.Add(new wpdFileInfo("f:" + name, "wpd_" + currentObject.Id, objSize, objDateStr));
                        }

                    }

                    // a path sequence along the targetFolder is needed; as long as currentFolder is the beginning of the targetFolder, we are on the right track
                    string latestFolder = Path.Combine(currentFolder, currentObject.Name);
                    if ( (targetFolder.StartsWith(latestFolder)) ) {
                        // next level starts with a new currentFolder
                        currentFolder = latestFolder;
                        EnumerateFolderContent(ref content, (PortableDeviceFolder)currentObject, currentFolder, targetFolder, ref retlist, ref targetFolderID);
                        // after returning from a deeper level, currentFolder needs to reset to the next higher level
                        currentFolder = Path.GetDirectoryName(currentFolder);
                    }
                }

            } while ( fetched > 0 );
        }

        // gets the full list of all objects from a WPD
        public PortableDeviceFolder GetContents() {
            PortableDeviceFolder root = new PortableDeviceFolder("DEVICE", "DEVICE");
            IPortableDeviceContent content;
            this._device.Content(out content);
            EnumerateContents(ref content, root);
            return root;
        }
        private static void EnumerateContents(ref IPortableDeviceContent content, PortableDeviceFolder parent) {
            // Get the properties of the object
            IPortableDeviceProperties properties;
            content.Properties(out properties);
            // Enumerate the items contained by the current object
            IEnumPortableDeviceObjectIDs objectIds;
            content.EnumObjects(0, parent.Id, null, out objectIds);
            uint fetched = 0;
            do {
                string objectId;
                objectIds.Next(1, out objectId, ref fetched);
                if ( fetched > 0 ) {
                    PortableDeviceObject currentObject = WrapObject(properties, objectId);
                    parent.Files.Add(currentObject);
                    if ( currentObject is PortableDeviceFolder ) {
                        EnumerateContents(ref content, (PortableDeviceFolder)currentObject);
                    }
                }
            } while ( fetched > 0 );
        }

        // DOWNLOAD file
        public bool DownloadFileFromWPD(PortableDeviceFile file, string saveToPath) {
            bool success = false;
            IPortableDeviceContent content;
            this._device.Content(out content);
            IPortableDeviceResources resources;
            content.Transfer(out resources);
            PortableDeviceApiLib.IStream wpdStream;
            uint optimalTransferSize = 0;
            _tagpropertykey property = new _tagpropertykey();
            property.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
            property.pid = 0;
            resources.GetStream(file.Id, ref property, 0, ref optimalTransferSize, out wpdStream);
            System.Runtime.InteropServices.ComTypes.IStream sourceStream = (System.Runtime.InteropServices.ComTypes.IStream)wpdStream;
            string filename = Path.GetFileName(file.Name);
            System.IO.Directory.CreateDirectory(saveToPath);
            FileStream targetStream = new FileStream(Path.Combine(saveToPath, filename), FileMode.Create, FileAccess.Write);
            try {
                unsafe {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    do {
                        sourceStream.Read(buffer, 1024, new IntPtr(&bytesRead));
                        //targetStream.Write(buffer, 0, 1024);
                        targetStream.Write(buffer, 0, bytesRead);
                    } while ( bytesRead > 0 );
                    targetStream.Close();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(sourceStream);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wpdStream);
                }
                success = true;
            } catch {
                success = false;
            }
            return success;
        }

        // DELETE file
        private static void StringToPropVariant(string value, out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue) {
            PortableDeviceApiLib.IPortableDeviceValues pValues = (PortableDeviceApiLib.IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValuesClass();
            _tagpropertykey WPD_OBJECT_ID = new _tagpropertykey();
            WPD_OBJECT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ID.pid = 2;
            pValues.SetStringValue(ref WPD_OBJECT_ID, value);
            pValues.GetValue(ref WPD_OBJECT_ID, out propvarValue);
        }
        public bool DeleteFile(PortableDeviceFile file) {
            bool success = false;
            // original
            //            IPortableDeviceContent content;
            //            this._device.Content(out content);
            PortableDeviceApiLib.tag_inner_PROPVARIANT variant = new PortableDeviceApiLib.tag_inner_PROPVARIANT();
            StringToPropVariant(file.Id, out variant);
            PortableDeviceApiLib.IPortableDevicePropVariantCollection objectIds = new PortableDeviceTypesLib.PortableDevicePropVariantCollection() as PortableDeviceApiLib.IPortableDevicePropVariantCollection;
            objectIds.Add(variant);
            // the next 2 line need to appear AFTER "StringToPropVariant(file.Id, out variant);" - otherwise content is reset to null
            IPortableDeviceContent content;
            this._device.Content(out content);
            try {
                content.Delete(0, objectIds, null);
                success = true;
            } catch {
                success = false;
            }
            return success;
        }

        private static void StringToPropCopyVariant(string value, out PortableDeviceApiLib.tag_inner_PROPVARIANT propvarValue) {
            PortableDeviceApiLib.IPortableDeviceValues pValues = (PortableDeviceApiLib.IPortableDeviceValues)new PortableDeviceTypesLib.PortableDeviceValuesClass();
            _tagpropertykey WPD_OBJECT_ID = new _tagpropertykey();

            //            WPD_OBJECT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            //            WPD_OBJECT_ID.pid = 12;

            WPD_OBJECT_ID.fmtid = new Guid(0xef1e43dd, 0xa9ed, 0x4341, 0x8b, 0xcc, 0x18, 0x61, 0x92, 0xae, 0xa0, 0x89);
            WPD_OBJECT_ID.pid = 9;

            //            WPD_OBJECT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            //            WPD_OBJECT_ID.pid = 2;
            pValues.SetStringValue(ref WPD_OBJECT_ID, value);

            pValues.GetValue(ref WPD_OBJECT_ID, out propvarValue);
        }
        public bool CopyInsideWPD(PortableDeviceFile file, string destinationFolderId) {
            bool success = false;

            PortableDeviceApiLib.tag_inner_PROPVARIANT variant = new PortableDeviceApiLib.tag_inner_PROPVARIANT();
            StringToPropCopyVariant(file.Id, out variant);
            PortableDeviceApiLib.IPortableDevicePropVariantCollection objectIds = new PortableDeviceTypesLib.PortableDevicePropVariantCollection() as PortableDeviceApiLib.IPortableDevicePropVariantCollection;
            objectIds.Add(variant);

            IPortableDeviceContent content;
            this._device.Content(out content);
            try {
                PortableDeviceApiLib.IPortableDevicePropVariantCollection res = new PortableDeviceTypesLib.PortableDevicePropVariantCollection() as PortableDeviceApiLib.IPortableDevicePropVariantCollection;
                content.Copy(objectIds, destinationFolderId, ref res);
                success = true;
            } catch ( Exception ex ) {
                MessageBox.Show("CopyInsideWPD(..) - " + ex.Message);
                success = false;
            }
            return success;
        }

        // COPY inside of the same WPD
        public bool CopyInsideWPD2(PortableDeviceFile file, string parentObjectId) {
            bool success = false;

            // WPD source
            IPortableDeviceContent contentSrc;
            this._device.Content(out contentSrc);
            IPortableDeviceResources resourcesSrc;
            contentSrc.Transfer(out resourcesSrc);
            PortableDeviceApiLib.IStream wpdStreamSrc;
            uint optimalTransferSizeSrc = 0;
            _tagpropertykey propertySrc = new _tagpropertykey();
            propertySrc.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
            propertySrc.pid = 0;
            resourcesSrc.GetStream(file.Id, ref propertySrc, 0, ref optimalTransferSizeSrc, out wpdStreamSrc);
            System.Runtime.InteropServices.ComTypes.IStream streamSrc = (System.Runtime.InteropServices.ComTypes.IStream)wpdStreamSrc;

            // WPD destination
            IPortableDeviceContent contentDst;
            this._device.Content(out contentDst);
            IPortableDeviceValues values = this.GetRequiredPropertiesForCopyWPD(file, parentObjectId);
            PortableDeviceApiLib.IStream tempStream;
            uint optimalTransferSizeBytes = 0;
            contentDst.CreateObjectWithPropertiesAndData(values, out tempStream, ref optimalTransferSizeBytes, null);
            System.Runtime.InteropServices.ComTypes.IStream streamDst = (System.Runtime.InteropServices.ComTypes.IStream)tempStream;

            try {
                unsafe {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    do {
                        // read from source
                        streamSrc.Read(buffer, 1024, new IntPtr(&bytesRead));

                        // write to destination
                        IntPtr pcbWritten = IntPtr.Zero;
                        if ( bytesRead > 0 ) {
                            streamDst.Write(buffer, bytesRead, pcbWritten);
                        }

                    } while ( bytesRead > 0 );
                }
                success = true;
                streamDst.Commit(0);
            } catch ( Exception ) {
                success = false;
            } finally {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempStream);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(streamSrc);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wpdStreamSrc);
            }

            return success;
        }
        private IPortableDeviceValues GetRequiredPropertiesForCopyWPD(PortableDeviceFile file, string parentObjectId) {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            _tagpropertykey WPD_OBJECT_PARENT_ID = new _tagpropertykey();
            WPD_OBJECT_PARENT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_PARENT_ID.pid = 3;
            values.SetStringValue(ref WPD_OBJECT_PARENT_ID, parentObjectId);

            long fileSize = this.GetFileSize(file.Id);
            _tagpropertykey WPD_OBJECT_SIZE = new _tagpropertykey();
            WPD_OBJECT_SIZE.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_SIZE.pid = 11;
            values.SetUnsignedLargeIntegerValue(WPD_OBJECT_SIZE, (ulong)fileSize);

            _tagpropertykey WPD_OBJECT_ORIGINAL_FILE_NAME = new _tagpropertykey();
            WPD_OBJECT_ORIGINAL_FILE_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ORIGINAL_FILE_NAME.pid = 12;
            values.SetStringValue(WPD_OBJECT_ORIGINAL_FILE_NAME, file.Name);

            _tagpropertykey WPD_OBJECT_NAME = new _tagpropertykey();
            WPD_OBJECT_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_NAME.pid = 4;
            values.SetStringValue(WPD_OBJECT_NAME, file.Name);

            return values;
        }


        //private int renameObject( string objectId, string newName ) 
        //    {
        //    int err = 0;

        //        CComPtr<IPortableDeviceValues> properties, values, results;
        //        IPortableDeviceValues propertyValues;

        //    err = CoCreateInstance( CLSID_PortableDeviceValues, NULL, CLSCTX_INPROC_SERVER, IID_IPortableDeviceValues, (VOID**) &properties );
        //    err = CoCreateInstance( CLSID_PortableDeviceValues, NULL, CLSCTX_INPROC_SERVER, IID_IPortableDeviceValues, (VOID**) &values );

        //    // Mount the command.
        //    err = properties->SetGuidValue( WPD_PROPERTY_COMMON_COMMAND_CATEGORY , WPD_COMMAND_OBJECT_PROPERTIES_SET.fmtid );
        //    err = properties->SetUnsignedIntegerValue( WPD_PROPERTY_COMMON_COMMAND_ID, WPD_COMMAND_OBJECT_PROPERTIES_SET.pid );

        //    // Set the values
        //    err = properties->SetStringValue( WPD_PROPERTY_OBJECT_PROPERTIES_OBJECT_ID, objectId );
        //    err = values->SetStringValue( WPD_OBJECT_ORIGINAL_FILE_NAME, newName );
        //    err = properties->SetIPortableDeviceValuesValue( WPD_PROPERTY_OBJECT_PROPERTIES_PROPERTY_VALUES, values );
        //    err = device->SendCommand( 0, properties, &results );

        //    return err;
        //}




        // file size
        private long GetFileSize(string objectId) {
            long objSize = 0;
            IPortableDeviceContent content;
            this._device.Content(out content);
            // Get the properties of the object
            IPortableDeviceProperties properties;
            content.Properties(out properties);
            PortableDeviceObject currentObject = WrapObject(properties, objectId);
            if ( currentObject is PortableDeviceFile ) {
                // get file properties
                IPortableDeviceKeyCollection keys;
                properties.GetSupportedProperties(currentObject.Id, out keys);
                IPortableDeviceValues values;
                properties.GetValues(currentObject.Id, keys, out values);
                // get file size
                _tagpropertykey property = new _tagpropertykey();
                property.fmtid = new Guid("EF6B490D-5CD8-437A-AFFC-DA8B60EE4A3C");
                property.pid = 11;   //WPD_OBJECT_SIZE;
                values.GetSignedLargeIntegerValue(property, out objSize);
            }
            return objSize;
        }

        // UPLOAD file
        public bool UploadFileToWPD(string fileName, string parentObjectId) {
            bool success = false;
            IPortableDeviceContent content;
            this._device.Content(out content);
            IPortableDeviceValues values = this.GetRequiredPropertiesForContentType(fileName, parentObjectId);
            PortableDeviceApiLib.IStream tempStream;
            uint optimalTransferSizeBytes = 0;
            content.CreateObjectWithPropertiesAndData(values, out tempStream, ref optimalTransferSizeBytes, null);
            System.Runtime.InteropServices.ComTypes.IStream targetStream = (System.Runtime.InteropServices.ComTypes.IStream)tempStream;
            try {
                using ( FileStream sourceStream = new FileStream(fileName, FileMode.Open, FileAccess.Read) ) {
                    byte[] buffer = new byte[optimalTransferSizeBytes];
                    int bytesRead;
                    do {
                        bytesRead = sourceStream.Read(buffer, 0, (int)optimalTransferSizeBytes);
                        IntPtr pcbWritten = IntPtr.Zero;
                        if ( bytesRead > 0 ) {
                            targetStream.Write(buffer, bytesRead, pcbWritten);
                        }
                    } while ( bytesRead > 0 );
                }
                targetStream.Commit(0);
                success = true;
            } catch {
                success = false;
            } finally {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(tempStream);
            }
            return success;
        }
        private IPortableDeviceValues GetRequiredPropertiesForContentType(string fileName, string parentObjectId) {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            _tagpropertykey WPD_OBJECT_PARENT_ID = new _tagpropertykey();
            WPD_OBJECT_PARENT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_PARENT_ID.pid = 3;
            values.SetStringValue(ref WPD_OBJECT_PARENT_ID, parentObjectId);

            FileInfo fileInfo = new FileInfo(fileName);
            _tagpropertykey WPD_OBJECT_SIZE = new _tagpropertykey();
            WPD_OBJECT_SIZE.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_SIZE.pid = 11;
            values.SetUnsignedLargeIntegerValue(WPD_OBJECT_SIZE, (ulong)fileInfo.Length);

            _tagpropertykey WPD_OBJECT_ORIGINAL_FILE_NAME = new _tagpropertykey();
            WPD_OBJECT_ORIGINAL_FILE_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ORIGINAL_FILE_NAME.pid = 12;
            values.SetStringValue(WPD_OBJECT_ORIGINAL_FILE_NAME, Path.GetFileName(fileName));

            _tagpropertykey WPD_OBJECT_NAME = new _tagpropertykey();
            WPD_OBJECT_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_NAME.pid = 4;
            values.SetStringValue(WPD_OBJECT_NAME, Path.GetFileName(fileName));

            return values;
        }

        public void CreateFolder(string fileName, string parentObjectId) {
            IPortableDeviceContent content;
            this._device.Content(out content);

            //folder info
            IPortableDeviceValues values = this.GetRequiredPropertiesForContentTypeFolder(fileName, parentObjectId);

            string objectId = "";
            content.CreateObjectWithPropertiesOnly(values, ref objectId);
        }
        private IPortableDeviceValues GetRequiredPropertiesForContentTypeFolder(string fileName, string parentObjectId) {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;

            _tagpropertykey WPD_OBJECT_PARENT_ID = new _tagpropertykey();
            WPD_OBJECT_PARENT_ID.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_PARENT_ID.pid = 3;
            values.SetStringValue(ref WPD_OBJECT_PARENT_ID, parentObjectId);

            //FileInfo fileInfo = new FileInfo(fileName);
            //var WPD_OBJECT_SIZE = new _tagpropertykey();
            //WPD_OBJECT_SIZE.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            //WPD_OBJECT_SIZE.pid = 11;
            //values.SetUnsignedLargeIntegerValue(WPD_OBJECT_SIZE, (ulong)fileInfo.Length);

            _tagpropertykey WPD_OBJECT_ORIGINAL_FILE_NAME = new _tagpropertykey();
            WPD_OBJECT_ORIGINAL_FILE_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_ORIGINAL_FILE_NAME.pid = 12;
            values.SetStringValue(WPD_OBJECT_ORIGINAL_FILE_NAME, Path.GetFileName(fileName));

            _tagpropertykey WPD_OBJECT_NAME = new _tagpropertykey();
            WPD_OBJECT_NAME.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_NAME.pid = 4;
            values.SetStringValue(WPD_OBJECT_NAME, Path.GetFileName(fileName));

            _tagpropertykey WPD_OBJECT_CONTENT_TYPE = new _tagpropertykey();
            WPD_OBJECT_CONTENT_TYPE.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_CONTENT_TYPE.pid = 7;
            Guid WPD_CONTENT_TYPE_FOLDER = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            values.SetGuidValue(WPD_OBJECT_CONTENT_TYPE, WPD_CONTENT_TYPE_FOLDER);

            _tagpropertykey WPD_OBJECT_FORMAT = new _tagpropertykey();
            WPD_OBJECT_FORMAT.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            WPD_OBJECT_FORMAT.pid = 6;
            Guid WPD_OBJECT_FORMAT_PROPERTIES_ONLY = new Guid(0x30010000, 0xAE6C, 0x4804, 0x98, 0xBA, 0xC5, 0x7B, 0x46, 0x96, 0x5F, 0xE7);
            values.SetGuidValue(WPD_OBJECT_FORMAT, WPD_OBJECT_FORMAT_PROPERTIES_ONLY);

            return values;
        }

        private static PortableDeviceObject WrapObject(IPortableDeviceProperties properties, string objectId) {
            IPortableDeviceKeyCollection keys;
            properties.GetSupportedProperties(objectId, out keys);

            IPortableDeviceValues values;
            properties.GetValues(objectId, keys, out values);

            // Get the name of the object
            string name;
            _tagpropertykey property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                                      0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 4;
            values.GetStringValue(property, out name);

            // Get the type of the object
            Guid contentType;
            property = new _tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                                      0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 7;
            values.GetGuidValue(property, out contentType);

            Guid folderType = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C,
                                      0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            Guid functionalType = new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98,
                                          0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);

            if ( contentType == folderType || contentType == functionalType ) {
                return new PortableDeviceFolder(objectId, name);
            }

            //return new PortableDeviceFile(objectId, name);

            property.pid = 12; //WPD_OBJECT_ORIGINAL_FILE_NAME
            values.GetStringValue(property, out name);
            return new PortableDeviceFile(objectId, name);
        }

        #endregion
    }

    public class PortableDeviceCollection : Collection<PortableDevice> {
        private readonly PortableDeviceManager _deviceManager;

        public PortableDeviceCollection() {
            this._deviceManager = new PortableDeviceManager();
        }

        public void Refresh() {
            this._deviceManager.RefreshDeviceList();

            // retrieve WPD devices --> need to modify Interop.PortableDeviceApi.lib
            // https://cgeers.wordpress.com/2011/05/22/enumerating-windows-portable-devices/
            // https://blogs.msdn.microsoft.com/dimeby8/2006/12/05/enumerating-wpd-devices-in-c/
            uint count = 1;
            this._deviceManager.GetDevices(null, ref count);
            string[] deviceIds = new string[count];
            this._deviceManager.GetDevices(deviceIds, ref count);
            foreach ( string deviceId in deviceIds ) {
                this.Add(new PortableDevice(deviceId));
            }
        }
    }

    public class PortableDeviceFile : PortableDeviceObject {
        public PortableDeviceFile(string id, string name) : base(id, name) {
        }
    }

    public class PortableDeviceFolder : PortableDeviceObject {
        public PortableDeviceFolder(string id, string name) : base(id, name) {
            this.Files = new List<PortableDeviceObject>();
        }

        public IList<PortableDeviceObject> Files { get; set; }
    }

    public abstract class PortableDeviceObject {
        protected PortableDeviceObject(string id, string name) {
            this.Id = id;
            this.Name = name;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }
    }

}