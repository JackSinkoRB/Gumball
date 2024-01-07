using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace MagneticScrollUtils
{
    [RequireComponent(typeof(Image))]
    public class ImageOffset : MonoBehaviour
    {
        private const string uiShader = "P11/UI_Sprite";
        private readonly int offsetProperty = Shader.PropertyToID("_Offset");

        [Tooltip("Field to change the offset from the inspector.")]
        [SerializeField] private Vector2 offset;

        [Tooltip("Allows automatic update of the offset based on speed.")]
        [SerializeField] private bool autoScroll;

        [Tooltip("Speed per axis, used to automatically scroll the offset value of the image over time.")]
        [ConditionalField(nameof(autoScroll)), SerializeField] private Vector2 speed;

        public Vector2 Offset
        {
            get => offset;
            set
            {
                AssignComponents();
                offset = value;
                image.materialForRendering.SetVector(offsetProperty, offset);
            }
        }

        private Image image;

        private void OnValidate()
        {
            UpdateOffset();
        }

        private void OnEnable()
        {
            UpdateOffset();
        }

        private void Update()
        {
            if (!autoScroll) return;
            Offset += speed * Time.deltaTime;
        }

        private void UpdateOffset()
        {
            Offset = offset;
        }

        private void AssignComponents()
        {
            image ??= GetComponent<Image>();

            if (!image.material.shader.name.Equals(uiShader))
            {
                image.material = new Material(Shader.Find(uiShader));
            }
        }

        public void SetAutoScroll(bool scroll, Vector2 speed)
        {
            autoScroll = scroll;
            this.speed = speed;
        }

    }
}