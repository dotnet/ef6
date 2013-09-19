// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;

    public class WorkOrderRouting
    {
        public virtual int WorkOrderID
        {
            get { return _workOrderID; }
            set
            {
                if (_workOrderID != value)
                {
                    if (WorkOrder != null
                        && WorkOrder.WorkOrderID != value)
                    {
                        WorkOrder = null;
                    }
                    _workOrderID = value;
                }
            }
        }

        private int _workOrderID;

        public virtual int ProductID { get; set; }

        public virtual short OperationSequence { get; set; }

        public virtual short LocationID
        {
            get { return _locationID; }
            set
            {
                if (_locationID != value)
                {
                    if (Location != null
                        && Location.LocationID != value)
                    {
                        Location = null;
                    }
                    _locationID = value;
                }
            }
        }

        private short _locationID;

        public virtual DateTime ScheduledStartDate { get; set; }

        public virtual DateTime ScheduledEndDate { get; set; }

        public virtual DateTime? ActualStartDate { get; set; }

        public virtual DateTime? ActualEndDate { get; set; }

        public virtual decimal? ActualResourceHrs { get; set; }

        public virtual decimal PlannedCost { get; set; }

        public virtual decimal? ActualCost { get; set; }

        public virtual DateTime ModifiedDate { get; set; }

        public virtual Location Location
        {
            get { return _location; }
            set
            {
                if (!ReferenceEquals(_location, value))
                {
                    var previousValue = _location;
                    _location = value;
                    FixupLocation(previousValue);
                }
            }
        }

        private Location _location;

        public virtual WorkOrder WorkOrder
        {
            get { return _workOrder; }
            set
            {
                if (!ReferenceEquals(_workOrder, value))
                {
                    var previousValue = _workOrder;
                    _workOrder = value;
                    FixupWorkOrder(previousValue);
                }
            }
        }

        private WorkOrder _workOrder;

        private void FixupLocation(Location previousValue)
        {
            if (previousValue != null
                && previousValue.WorkOrderRoutings.Contains(this))
            {
                previousValue.WorkOrderRoutings.Remove(this);
            }

            if (Location != null)
            {
                if (!Location.WorkOrderRoutings.Contains(this))
                {
                    Location.WorkOrderRoutings.Add(this);
                }
                if (LocationID != Location.LocationID)
                {
                    LocationID = Location.LocationID.Value;
                }
            }
        }

        private void FixupWorkOrder(WorkOrder previousValue)
        {
            if (previousValue != null
                && previousValue.WorkOrderRoutings.Contains(this))
            {
                previousValue.WorkOrderRoutings.Remove(this);
            }

            if (WorkOrder != null)
            {
                if (!WorkOrder.WorkOrderRoutings.Contains(this))
                {
                    WorkOrder.WorkOrderRoutings.Add(this);
                }
                if (WorkOrderID != WorkOrder.WorkOrderID)
                {
                    WorkOrderID = WorkOrder.WorkOrderID;
                }
            }
        }
    }
}
