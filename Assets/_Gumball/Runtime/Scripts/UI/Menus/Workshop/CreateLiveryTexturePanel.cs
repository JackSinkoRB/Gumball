using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CreateLiveryTexturePanel : AnimatedPanel
    {

        [SerializeField] private Transform optionButtonHolder;
        [SerializeField] private DecalOptionButton optionButtonPrefab;
        [SerializeField] private DecalCategoryHeaderFilter headerFilter;
        [SerializeField] private MultiImageButton selectButton;
        
        [SerializeField, ReadOnly] private DecalOptionButton selectedOption;

        protected override void OnShow()
        {
            base.OnShow();
            
            this.PerformAtEndOfFrame(() =>
            {
                headerFilter.Select(0);
                SelectDecalButton(null);
            });
        }

        public void PopulateDecals()
        {
            foreach (Transform child in optionButtonHolder)
                child.gameObject.Pool();

            foreach (DecalTexture decalTexture in headerFilter.CurrentSelected.DecalTextures)
            {
                DecalOptionButton instance = optionButtonPrefab.gameObject.GetSpareOrCreate<DecalOptionButton>(optionButtonHolder);
                instance.Initialise(decalTexture);
                instance.transform.SetAsLastSibling();
            }
        }
        
        public void SelectDecalButton(DecalOptionButton option)
        {
            if (selectedOption != null)
                selectedOption.OnDeselect();
            
            selectedOption = option;
            if (selectedOption != null)
                selectedOption.OnSelect();

            selectButton.interactable = selectedOption != null;
        }

        public void OnClickSelectButton()
        {
            Hide();
            
            LiveDecal decal = DecalEditor.Instance.CreateLiveDecal(headerFilter.CurrentSelected, selectedOption.DecalTexture);
            
            //move to the car position
            RaycastHit[] results = new RaycastHit[1];
            const float maxDistance = 1000;
            int hits = Physics.BoxCastNonAlloc(Camera.main.transform.position, new Vector3(0.1f, 10, 1), (WarehouseManager.Instance.CurrentCar.transform.position - Camera.main.transform.position).normalized,  results, Quaternion.LookRotation((WarehouseManager.Instance.CurrentCar.transform.position - Camera.main.transform.position).normalized), maxDistance, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.PaintableMesh));
            if (hits > 0)
            {
                RaycastHit hit = results[0];
                Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward - hit.normal, Vector3.up);
                decal.UpdatePosition(DecalEditor.Instance.CurrentCar.transform.InverseTransformPoint(hit.point), hit.normal, rotation);
            }
            
            decal.SetValid(hits > 0);
            
            GlobalLoggers.DecalsLogger.Log($"Hit {(results.Length > 0 ? results[0] : "nothing")} ({hits} total) paintable meshes looking from {Camera.main.transform.position} to {((WarehouseManager.Instance.CurrentCar.transform.position - Camera.main.transform.position).normalized) * maxDistance}");
            
            DecalStateManager.LogStateChange(new DecalStateManager.CreateStateChange(decal));
            DecalEditor.Instance.SelectLiveDecal(decal);
        }
        
    }
}
