using System;
using System.Collections;
using System.Collections.Generic;
using PaintIn3D;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public struct PaintableMesh
    {
        [SerializeField] private P3dPaintable paintable;
        [SerializeField] private P3dMaterialCloner materialCloner;
        [SerializeField] private P3dPaintableTexture paintableTexture;

        public P3dPaintable Paintable => paintable;
        public P3dMaterialCloner MaterialCloner => materialCloner;
        public P3dPaintableTexture PaintableTexture => paintableTexture;
        
        public PaintableMesh(P3dPaintable paintable, P3dMaterialCloner materialCloner, P3dPaintableTexture paintableTexture)
        {
            this.paintable = paintable;
            this.materialCloner = materialCloner;
            this.paintableTexture = paintableTexture;
        }
    }
}
