using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSViewer.Common
{
    public class Transform : INotifyPropertyChanged, IEnumerable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> The parent of this transform that this transform moves locally to. null if a root object. </summary>
        public Transform Parent
        {
            get { return m_parent; }
            set
            {
                // Remove ourself from our current parents children (if we have any)
                if (m_parent != null)
                {
                    m_parent.m_children.Remove(this);
                    m_parent.OnPropertyChanged("ChildCount");
                }

                // Then assign our new parent, and add ourself to that new parent's children (if non null)
                m_parent = value;

                if (m_parent != null)
                {
                    m_parent.m_children.Add(this);
                    OnPropertyChanged("ChildCount");
                }

                OnPropertyChanged("Parent");
            }
        }

        /// <summary> Position of the object in global space. </summary>
        public Vector3 Position
        {
            get
            {
                // Walk the tree until there's no more parents and add up our local position distributed across all.
                Vector3 transformedPoint = LocalPosition;
                Transform curParent = m_parent;
                while (curParent != null)
                {
                    transformedPoint = Vector3.Transform(transformedPoint, curParent.LocalRotation);
                    transformedPoint += curParent.LocalPosition;

                    curParent = curParent.m_parent;
                }

                return transformedPoint;
            }
            set
            {
                Vector3 transformedPoint = value;
                Transform curParent = m_parent;
                while (curParent != null)
                {
                    // Calculate the inverse of the parent's rotation so we can subtract that rotation.
                    Quaternion inverseRot = curParent.LocalRotation;
                    inverseRot.Invert();

                    transformedPoint -= curParent.LocalPosition;
                    transformedPoint = Vector3.Transform(transformedPoint, inverseRot);

                    curParent = curParent.m_parent;
                }

                m_localPosition = transformedPoint;
                OnPropertyChanged("Position");
            }
        }

        /// <summary> Rotation of the object in global space. </summary>
        public Quaternion Rotation
        {
            get
            {
                // Walk the tree until no more parents adding up the local rotations of each.
                Quaternion transformedRot = LocalRotation;
                Transform curParent = m_parent;
                while (curParent != null)
                {
                    transformedRot = curParent.LocalRotation * transformedRot;

                    curParent = curParent.m_parent;
                }

                return transformedRot;
            }
            set
            {
                Quaternion transformedRot = value;
                Transform curParent = m_parent;
                while (curParent != null)
                {
                    // Calculate the inverse of the parents rotation so we can subtract that rotation.
                    Quaternion inverseRot = curParent.LocalRotation;
                    inverseRot.Invert();
                    transformedRot = inverseRot * transformedRot;

                    curParent = curParent.m_parent;
                }

                m_localRotation = transformedRot;
                OnPropertyChanged("Rotation");
            }
        }

        /// <summary> Local scale of the object. Global scale is not supported. </summary>
        public Vector3 LocalScale
        {
            get { return m_localScale; }
            set
            {
                m_localScale = value;
                OnPropertyChanged("LocalScale");
            }
        }

        /// <summary> Local position relative to the parent. Equivelent to Position if parent is null. </summary>
        public Vector3 LocalPosition
        {
            get { return m_localPosition; }
            set
            {
                m_localPosition = value;
                OnPropertyChanged("LocalPosition");
            }
        }

        /// <summary> Local rotation relative to the parent. Equivelent to Rotation if parent is null. </summary>
        public Quaternion LocalRotation
        {
            get { return m_localRotation; }
            set
            {
                m_localRotation = value;
                OnPropertyChanged("LocalRotation");
            }
        }

        /// <summary> The number of children in this transform. </summary>
        public int ChildCount
        {
            get { return m_children.Count; }
        }

        /// <summary> The local up axis of this object as a direction in world space. </summary>
        public Vector3 Up
        {
            get { return Vector3.Transform(Vector3.UnitY, Rotation); }
        }

        /// <summary> The local right axis of this object as a direction in world space. </summary>
        public Vector3 Right
        {
            get { return Vector3.Transform(Vector3.UnitX, Rotation); }
        }

        /// <summary> The local forward axis of this object as a direction in world space. </summary>
        public Vector3 Forward
        {
            get { return Vector3.Transform(Vector3.UnitZ, Rotation); }
        }

        /// <summary>
        /// Returns a child transform by the specified index. Throws <see cref="ArgumentOutOfRangeException"/> if an invalid child index is specified. </summary>
        /// <param name="index">Index of the child to return. Get total child count via <see cref="ChildCount"/>.</param>
        /// <returns></returns>
        public Transform GetChild(int index)
        {
            if (index < 0 || index >= ChildCount)
                throw new ArgumentOutOfRangeException("index");

            return m_children[index];
        }

        private Transform m_parent;
        private Vector3 m_localPosition;
        private Vector3 m_localScale;
        private Quaternion m_localRotation;

        /// <summary> Children of this transform. It's marked as private and only accessible via GetEnumerator as we don't want people arbitrarily adding children. </summary>
        private List<Transform> m_children;

        public Transform()
        {
            LocalPosition = Vector3.Zero;
            LocalRotation = Quaternion.Identity;
            LocalScale = Vector3.One;
            Parent = null;

            m_children = new List<Transform>();
        }

        public void Rotate(Vector3 axis, float angleInDegrees)
        {
            Quaternion rotQuat = Quaternion.RotationAxis(axis, angleInDegrees * VSTools.Deg2Rad);
            Rotation = rotQuat * Rotation;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerator GetEnumerator()
        {
            foreach (Transform trns in m_children)
            {
                yield return trns;
            }
        }

        public override bool Equals(object o)
        {
            Transform otherTrns = o as Transform;
            if (otherTrns == null)
                return false;

            return Position == otherTrns.Position && Rotation == otherTrns.Rotation && LocalScale == otherTrns.LocalScale;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ Rotation.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Transform (Position: {0} Rotation: {1} LocalScale: {2})", Position, Rotation, LocalScale);
        }
    }
}
