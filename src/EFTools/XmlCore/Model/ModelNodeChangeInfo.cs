// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    internal enum ModelNodeChangeType
    {
        Added,
        Deleted,
        Changed
    };

    internal class ModelNodeChangeInfo
    {
        private readonly ModelNodeChangeType _changeType;
        private readonly EFObject _modelNode;

        public ModelNodeChangeInfo(EFObject modelNode, ModelNodeChangeType changeType)
        {
            _changeType = changeType;
            _modelNode = modelNode;
        }

        public ModelNodeChangeType ChangeType
        {
            get { return _changeType; }
        }

        public EFObject ModelNode
        {
            get { return _modelNode; }
        }
    }
}
