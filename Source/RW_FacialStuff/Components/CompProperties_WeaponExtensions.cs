﻿namespace FacialStuff.Components
{
    using UnityEngine;

    using Verse;

    public class CompProperties_WeaponExtensions : CompProperties
    {
        #region Public Fields

        public Vector3 FirstHandPosition ;

        public Vector3 SecondHandPosition ;

        public Vector3 WeaponPositionOffset ;

        public float AttackAngleOffset;

        #endregion Public Fields

        #region Public Constructors

        public CompProperties_WeaponExtensions()
        {
            this.compClass = typeof(CompWeaponExtensions);
        }

        #endregion Public Constructors
    }
}
