using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardSwipe
{
    public class CardSwipeEffect : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private const float TIME_TO_RESET = 0.2f;
        [SerializeField] private const float SWIPE_SPEED = 4f;
        [SerializeField] private const float PERCENT_SCREEN_TO_SWIPE = 0.5f;
        [SerializeField] private const float CARD_ROTATION_ON_SWIPE = 30f;

        public bool SwipedLeft
        {
            get => _swipedLeft;
            set => _swipedLeft = value;
        }

        public event Action CardSelectAction;

        private void Awake() 
        {
            _initialPosition = transform.localPosition;
            _cardImage = GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _initialLocalPosition = transform.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.localPosition = new Vector2(transform.localPosition.x + eventData.delta.x, transform.localPosition.y);

            var movingLeft = transform.localPosition.x - _initialLocalPosition.x > 0;
            var movementDelta = movingLeft ?
                _initialLocalPosition.x + transform.localPosition.x :
                _initialLocalPosition.x - transform.localPosition.x;

            var rotatonDelta = movingLeft ?
                -CARD_ROTATION_ON_SWIPE :
                CARD_ROTATION_ON_SWIPE;

            var deltaRotation = Mathf.Lerp(0, rotatonDelta, movementDelta / (Screen.width/2)); 

            transform.localEulerAngles = new Vector3(0,0, deltaRotation); 

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var deltaMove = transform.localPosition.x - _initialLocalPosition.x;
            _distanceMoved = Mathf.Abs(deltaMove);

            if(_distanceMoved < PERCENT_SCREEN_TO_SWIPE * Screen.width)
            {
                transform.localPosition = _initialLocalPosition;
                transform.localEulerAngles = Vector3.zero;
            }else
            {
                _swipedLeft = transform.localPosition.x < _initialLocalPosition.x;

                CardSelectAction?.Invoke();
                StartCoroutine(MoveCard());
            }
            
        }

        IEnumerator MoveCard()
        {
            float timeElapsed = 0f;

            while(_cardImage.color.a != 0)
            {
                timeElapsed += Time.deltaTime;

                if(_swipedLeft)
                {
                    transform.localPosition = new Vector3(
                        Mathf.SmoothStep(transform.localPosition.x, transform.localPosition.x - Screen.width,
                         SWIPE_SPEED * timeElapsed), transform.localPosition.y, 0);
                    Debug.Log("Going left");
                }else
                {
                    transform.localPosition = new Vector3(
                        Mathf.SmoothStep(transform.localPosition.x, transform.localPosition.x + Screen.width,
                         SWIPE_SPEED * timeElapsed), transform.localPosition.y, 0);
                }

                // So stupid unity why cant i just modify the alpha???? why must i be forced to do this
                _cardImage.color = new Color(_cardImage.color.r, _cardImage.color.g, _cardImage.color.b, Mathf.SmoothStep(1,0, SWIPE_SPEED * timeElapsed));
                yield return null;
            }

            StartCoroutine(ResetCard());
        }

        IEnumerator ResetCard()
        {
            _swipedLeft = false;

            yield return new WaitForSeconds(TIME_TO_RESET);

            transform.localPosition = _initialPosition;
            transform.localEulerAngles = Vector3.zero;
            _cardImage.color = new Color(_cardImage.color.r, _cardImage.color.g, _cardImage.color.b, 1);
        }

        private Vector3 _initialPosition;
        private Vector3 _initialLocalPosition;
        private float _distanceMoved;
        private Image _cardImage;
        private bool _swipedLeft;
    }
}
