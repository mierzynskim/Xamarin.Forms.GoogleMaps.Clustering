using Android.Runtime;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Google.Maps.Android.Clustering.Algo
{
    public partial class AbstractAlgorithm
    {
        public abstract unsafe bool AddItem(global::Java.Lang.Object item);
        public abstract unsafe bool RemoveItem(global::Java.Lang.Object item);
        public abstract unsafe bool UpdateItem(global::Java.Lang.Object item);
    }

    internal abstract partial class AbstractAlgorithmInvoker
    {        
        [Register("addItem", "(Lcom/google/maps/android/clustering/ClusterItem;)Z", "GetAddItem_Lcom_google_maps_android_clustering_ClusterItem_Handler")]
        public override unsafe bool AddItem(global::Java.Lang.Object item)
        {
            const string __id = "addItem.(Lcom/google/maps/android/clustering/ClusterItem;)Z";
            IntPtr native_item = JNIEnv.ToLocalJniHandle(item);
            try
            {
                JniArgumentValue* __args = stackalloc JniArgumentValue[1];
                __args[0] = new JniArgumentValue(native_item);
                var __rm = _members.InstanceMethods.InvokeAbstractBooleanMethod(__id, this, __args);
                return __rm;
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_item);
                global::System.GC.KeepAlive(item);
            }
        }

        [Register("removeItem", "(Lcom/google/maps/android/clustering/ClusterItem;)Z", "GetRemoveItem_Lcom_google_maps_android_clustering_ClusterItem_Handler")]
        public override unsafe bool RemoveItem(global::Java.Lang.Object item)
        {
            const string __id = "removeItem.(Lcom/google/maps/android/clustering/ClusterItem;)Z";
            IntPtr native_item = JNIEnv.ToLocalJniHandle(item);
            try
            {
                JniArgumentValue* __args = stackalloc JniArgumentValue[1];
                __args[0] = new JniArgumentValue(native_item);
                var __rm = _members.InstanceMethods.InvokeAbstractBooleanMethod(__id, this, __args);
                return __rm;
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_item);
                global::System.GC.KeepAlive(item);
            }
        }


        [Register("updateItem", "(Lcom/google/maps/android/clustering/ClusterItem;)Z", "GetUpdateItem_Lcom_google_maps_android_clustering_ClusterItem_Handler")]
        public override unsafe bool UpdateItem(global::Java.Lang.Object item)
        {
            const string __id = "updateItem.(Lcom/google/maps/android/clustering/ClusterItem;)Z";
            IntPtr native_item = JNIEnv.ToLocalJniHandle(item);
            try
            {
                JniArgumentValue* __args = stackalloc JniArgumentValue[1];
                __args[0] = new JniArgumentValue(native_item);
                var __rm = _members.InstanceMethods.InvokeAbstractBooleanMethod(__id, this, __args);
                return __rm;
            }
            finally
            {
                JNIEnv.DeleteLocalRef(native_item);
                global::System.GC.KeepAlive(item);
            }
        }
    }
}
