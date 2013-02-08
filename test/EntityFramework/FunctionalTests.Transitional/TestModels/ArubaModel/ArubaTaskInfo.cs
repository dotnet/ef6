// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    public class ArubaTaskInfo
    {
        public long Passed { get; set; }
        public long Failed { get; set; }
        public long Investigates { get; set; }
        public long Improvements { get; set; }
    }
}
