﻿using SATools.SAModel.ModelData;
using SATools.SAModel.ObjData;
using System;
using System.Collections.ObjectModel;
using System.Numerics;

namespace SAModel.WPF.Inspector.Viewmodel.InspectorViewmodels.ObjectData
{
    internal class IVmNJObject : InspectorViewModel
    {
        protected override Type ViewmodelType
            => typeof(NJObject);

        private NJObject NJObject
            => (NJObject)_source;

        [Tooltip("C label of the NJObject Object")]
        public string Name
        {
            get => NJObject.Name;
            set => NJObject.Name = value;
        }

        [Tooltip("Parent Object")]
        public NJObject Parent
        {
            get => NJObject.Parent;
            set => value.AddChild(NJObject);
        }

        [Tooltip("Children objects")]
        public ReadOnlyCollection<NJObject> Children 
            => NJObject.Children;

        [Tooltip("Mesh information")]
        public Attach Attach
        {
            get => NJObject.Attach;
            set => NJObject.Attach = value;
        }

        [DisplayName("Has Weight")]
        [Tooltip("Whether the model is weighted")]
        public bool HasWeight
            => NJObject.HasWeight;
        

        [Tooltip("World space position")]
        public Vector3 Position
        {
            get => NJObject.Position;
            set
            {
                NJObject.Position = value;
                OnPropertyChanged(nameof(LocalMatrix));
                OnPropertyChanged(nameof(WorldMatrix));
            }
        }

        [Tooltip("World space euler rotation")]
        public Vector3 Rotation
        {
            get => NJObject.Rotation;
            set
            {
                NJObject.Rotation = value;
                OnPropertyChanged(nameof(QuaternionRotation));
                OnPropertyChanged(nameof(LocalMatrix));
                OnPropertyChanged(nameof(WorldMatrix));
            }
        }

        [DisplayName("Quaternion Rotation")]
        [Tooltip("Quaternion world space euler rotation")]
        public Quaternion QuaternionRotation
        {
            get => NJObject.QuaternionRotation;
            set
            {
                NJObject.QuaternionRotation = value;
                OnPropertyChanged(nameof(Rotation));
                OnPropertyChanged(nameof(LocalMatrix));
                OnPropertyChanged(nameof(WorldMatrix));
            }
        }

        [Tooltip("World space scale")]
        public Vector3 Scale
        {
            get => NJObject.Scale;
            set
            {
                NJObject.Scale = value;
                OnPropertyChanged(nameof(LocalMatrix));
                OnPropertyChanged(nameof(WorldMatrix));
            }
        }

        [DisplayName("Local Matrix")]
        [Tooltip("Local transform matrix created from the transform properties")]
        public Matrix4x4 LocalMatrix
            => NJObject.LocalMatrix;

        [DisplayName("World Matrix")]
        [Tooltip("World transform matrix based on local matrix and parent world matrix")]
        public Matrix4x4 WorldMatrix
            => NJObject.GetWorldMatrix();

        [DisplayName("Rotate ZYX")]
        [Tooltip("Inverted Rotational order")]
        public bool RotateZYX
        {
            get => NJObject.RotateZYX;
            set => NJObject.RotateZYX = value;
        }

        [Tooltip("Whether the object is animatable")]
        public bool Animate
        {
            get => NJObject.Animate;
            set => NJObject.Animate = value;
        }

        [Tooltip("Whether the object is morphable")]
        public bool Morph
        {
            get => NJObject.Morph;
            set => NJObject.Morph = value;
        }

        [Tooltip("The written object attributes, which are determined by the other properties")]
        public ObjectAttributes Attributes
            => NJObject.Attributes;

        public IVmNJObject() : base() { }

        public IVmNJObject(NJObject source) : base(source) { }
    }
}