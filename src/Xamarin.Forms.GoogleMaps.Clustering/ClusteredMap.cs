using System;
using System.Collections.Generic;

namespace Xamarin.Forms.GoogleMaps.Clustering
{
    /// <summary>
    /// Defines the <see cref="ClusteredMap" />
    /// </summary>
    public class ClusteredMap : Map
    {
        #region Variables

        /// <summary>
        /// Defines the ClusterOptionsProperty
        /// </summary>
        public static readonly BindableProperty ClusterOptionsProperty = BindableProperty.Create(nameof(ClusterOptionsProperty),
            typeof(ClusterOptions),
            typeof(ClusteredMap),
            default(ClusterOptions));

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteredMap"/> class.
        /// </summary>
        public ClusteredMap()
        {
            ClusterOptions = new ClusterOptions();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the OnCluster
        /// </summary>
        internal Action OnCluster { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PendingClusterRequest
        /// </summary>
        internal bool PendingClusterRequest { get; set; }

        /// <summary>
        /// Gets or sets the ClusterOptions
        /// </summary>
        public ClusterOptions ClusterOptions { get => (ClusterOptions)GetValue(ClusterOptionsProperty); set => SetValue(ClusterOptionsProperty, value); }

        #endregion

        #region Public

        /// <summary>
        /// The Cluster
        /// </summary>
        public void Cluster()
        {
            SendCluster();
        }

        #endregion

        #region Private

        /// <summary>
        /// The SendCluster
        /// </summary>
        private void SendCluster()
        {
            if (OnCluster != null)
                OnCluster.Invoke();
            else
                PendingClusterRequest = true;
        }

        #endregion

        /// <summary>
        /// Defines the on cluster click eventhandler
        /// </summary>
        public event EventHandler<ClusterClickedEventArgs> ClusterClicked;

        /// <summary>
        ///Send the cluster click event with args
        /// </summary>
        /// <param name="itemsCount">The itemsCount<see cref="int"/></param>
        /// <param name="pins">The pins<see cref="IEnumerable{Pin}"/></param>
        /// <param name="position">The position<see cref="Position"/></param>
        internal void SendClusterClicked(int itemsCount, IEnumerable<Pin> pins, Position position)
        {
            var args = new ClusterClickedEventArgs(itemsCount, pins, position);
            ClusterClicked?.Invoke(this, args);
        }
    }
}
