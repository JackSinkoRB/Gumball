using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class AvatarBody : MonoBehaviour
    {
        
        [SerializeField] private AvatarBodyType bodyType;

        [SerializeField] private AvatarCosmetic[] cosmetics;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private Avatar avatarBelongsTo;

        public AvatarBodyType BodyType => bodyType;
        public AvatarCosmetic[] Cosmetics => cosmetics;
        
        public void Initialise(Avatar avatar)
        {
            avatarBelongsTo = avatar;

            foreach (AvatarCosmetic cosmetic in cosmetics)
            {
                cosmetic.Initialise(avatar);
            }
        }

    }
}
