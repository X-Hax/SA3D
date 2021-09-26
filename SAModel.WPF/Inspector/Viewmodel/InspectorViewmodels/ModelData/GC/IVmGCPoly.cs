﻿using SATools.SAModel.ModelData.GC;
using System;

namespace SATools.SAModel.WPF.Inspector.Viewmodel.InspectorViewmodels.ModelData.GC
{
    internal class IVmGCPoly : InspectorViewModel
    {
        protected override Type ViewmodelType
            => typeof(Poly);

        private Poly Poly
            => (Poly)_source;

        public PolyType PolyType
            => Poly.Type;

        public Corner[] Corners
            => Poly.Corners;

        public IVmGCPoly() : base() { }

        public IVmGCPoly(Poly source) : base(source) { }
    }
}
