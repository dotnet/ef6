// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeleteReferentialConstraintPropertyRefCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Deletes the passed in PropertyRef in a referential constraint
        /// </summary>
        /// <param name="property"></param>
        internal DeleteReferentialConstraintPropertyRefCommand(PropertyRef property)
            : base(property)
        {
        }

        protected PropertyRef PropertyRef
        {
            get
            {
                var elem = EFElement as PropertyRef;
                Debug.Assert(elem != null, "underlying element does not exist or is not a PropertyRef");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // find the ordinal pair of this property ref in the dependent section

            var role = PropertyRef.Parent as ReferentialConstraintRole;
            if (role != null)
            {
                var rc = role.Parent as ReferentialConstraint;
                if (rc != null)
                {
                    // find the "other" ReferentialConstraint
                    var other = rc.Principal;
                    if (rc.Principal == role)
                    {
                        other = rc.Dependent;
                    }
                    Debug.Assert(
                        (other == rc.Principal && role == rc.Dependent) || (role == rc.Principal && other == rc.Dependent),
                        "Why aren't both RefConstraintRoles being used?");

                    // 
                    //  Since principal & dependent property refs are paired up based on document order, 
                    //  we get the ordinal position of the anti-dep element, and then find its peer
                    //
                    var i = GetOrdinalPosition(PropertyRef);
                    var otherPRef = GetIthPropertyRef(other, i);

                    Debug.Assert(i == GetOrdinalPosition(otherPRef), "Unexpected ordinal positions for property refs!");

                    //
                    // we found the two peers to delete, so we want to delete them.
                    // don't call "DeleteInTransaction" here, as that will call GetDeleteCommand which will
                    // return this.  We want to use a straight "DeleteEFElementCommand" here.
                    //
                    Command cmd1 = new DeleteEFElementCommand(PropertyRef);
                    Command cmd2 = new DeleteEFElementCommand(otherPRef);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd1);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd2);
                }
            }
        }

        // returns the ordinal position of the xelement 
        private static int GetOrdinalPosition(PropertyRef pr)
        {
            var i = 0;
            var parent = pr.Parent as PropertyRefContainer;
            foreach (var p in parent.PropertyRefs)
            {
                if (p == pr)
                {
                    return i;
                }
                ++i;
            }
            Debug.Fail("couldn't find property ref in it's container!");
            return -1;
        }

        private static PropertyRef GetIthPropertyRef(PropertyRefContainer prc, int i)
        {
            var curr = 0;
            foreach (var pr in prc.PropertyRefs)
            {
                if (curr == i)
                {
                    Debug.Assert(i == GetOrdinalPosition(pr), "non-equal values for peer property refs!");
                    return pr;
                }
                ++curr;
            }
            Debug.Fail("couldn't find i'th property ref container!");
            return null;
        }
    }
}
