using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MagneticScrollUtils
{

	public class MagneticScrollDragger : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{

		private MagneticScroll magneticScroll;

		private void Awake()
		{
			magneticScroll = GetComponentInParent<MagneticScroll>();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			magneticScroll.OnPointerDown(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			magneticScroll.OnPointerUp(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			magneticScroll.OnDrag(eventData);

			//if the player drags while clicking, make sure it doesn't call the onClick
			eventData.eligibleForClick = false;
		}

	}
}