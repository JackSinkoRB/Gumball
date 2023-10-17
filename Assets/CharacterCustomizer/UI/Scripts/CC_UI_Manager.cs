using System.Collections.Generic;
using UnityEngine;

namespace CC
{
    public class CC_UI_Manager : MonoBehaviour
    {
        public static CC_UI_Manager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        [Tooltip("The parent object of your customizable characters")]
        public GameObject CharacterParent;

        public List<AudioClip> UISounds = new List<AudioClip>();

        private int characterIndex = 0;

        public void Start()
        {
            SetActiveCharacter(0);
        }

        public void playUIAudio(int Index)
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource && UISounds.Count > Index) audioSource.clip = UISounds[Index]; audioSource.Play();
        }

        public void SetActiveCharacter(int i)
        {
            characterIndex = i;

            for (int j = 0; j < CharacterParent.transform.childCount; j++)
            {
                //Set character active state
                CharacterParent.transform.GetChild(j).gameObject.SetActive(i == j);
                //Fetch menu from the character's component and set active state
                CharacterParent.transform.GetChild(j).GetComponent<CharacterCustomization>().UI.SetActive(i == j);
            }
        }

        public void characterNext()
        {
            SetActiveCharacter(characterIndex == CharacterParent.transform.childCount - 1 ? 0 : characterIndex + 1);
        }

        public void characterPrev()
        {
            SetActiveCharacter(characterIndex == 0 ? CharacterParent.transform.childCount - 1 : characterIndex - 1);
        }
    }
}