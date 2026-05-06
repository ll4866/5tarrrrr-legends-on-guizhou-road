using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LLMUnity;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LLMUnitySamples
{
    public class ChatBot : MonoBehaviour
    {
        public Transform chatContainer;
        public Color playerColor = new Color32(75, 70, 80, 255);
        public Color aiColor = new Color32(70, 80, 80, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public LLMAgent llmAgent;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;

        private InputBubble inputBubble;
        private List<Bubble> chatBubbles = new List<Bubble>();
        private bool blockInput = true;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;

        void Start()
        {
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };

            var aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            inputBubble = new InputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);

            inputBubble.AddSubmitListener(OnInputFieldSubmit);
            inputBubble.AddValueChangedListener(OnValueChanged);

            inputBubble.setInteractable(true);

            stopButton.gameObject.SetActive(true);

            ShowLoadedMessages();
            _ = llmAgent.Warmup(WarmUpCallback);

            StartCoroutine(FocusNextFrame());
        }

        void Update()
        {
            // ===== UNIVERSAL ENTER (Mac + iPad + Editor) =====
#if ENABLE_INPUT_SYSTEM
            bool enterPressed =
                Keyboard.current != null &&
                Keyboard.current.enterKey.wasPressedThisFrame;
#else
    bool enterPressed = Input.GetKeyDown(KeyCode.Return);
#endif

            if (enterPressed && !blockInput && warmUpDone)
            {
                SendMessage();
            }

            CleanupBubbles();
        }

        void SendMessage()
        {
            string text = inputBubble.GetText();
            OnInputFieldSubmit(text);
        }

        void OnInputFieldSubmit(string newText)
        {
            if (blockInput || !warmUpDone)
                return;

            if (string.IsNullOrWhiteSpace(newText))
                return;

            blockInput = true;

            string message = newText.Replace("\n", "");

            // 🔥 release keyboard / focus (iPad + editor safe)
            EventSystem.current.SetSelectedGameObject(null);

            AddBubble(message, true);
            Bubble aiBubble = AddBubble("...", false);

            llmAgent.Chat(message, aiBubble.SetText, AllowInput);

            inputBubble.SetText("");

            StartCoroutine(FocusNextFrame());
        }

        IEnumerator FocusNextFrame()
        {
            yield return null;
            inputBubble.ActivateInputField();
        }

        public void AllowInput()
        {
            blockInput = false;
            StartCoroutine(FocusNextFrame());
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            inputBubble.SetPlaceHolderText("Message me");
            AllowInput();
        }

        void OnValueChanged(string text) { }

        Bubble AddBubble(string message, bool isPlayer)
        {
            Bubble bubble = new Bubble(
                chatContainer,
                isPlayer ? CreatePlayerUI() : CreateAIUI(),
                isPlayer ? "PlayerBubble" : "AIBubble",
                message
            );

            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        BubbleUI CreatePlayerUI()
        {
            return new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
        }

        BubbleUI CreateAIUI()
        {
            var ui = CreatePlayerUI();
            ui.bubbleColor = aiColor;
            ui.leftPosition = 1;
            return ui;
        }

        void ShowLoadedMessages()
        {
            for (int i = 1; i < llmAgent.chat.Count; i++)
                AddBubble(llmAgent.chat[i].content, i % 2 == 1);
        }

        void CleanupBubbles()
        {
            if (lastBubbleOutsideFOV != -1)
            {
                for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                    chatBubbles[i].Destroy();

                chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                lastBubbleOutsideFOV = -1;
            }
        }

        public void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;

            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                var bubble = chatBubbles[i];
                RectTransform rt = bubble.GetRectTransform();

                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);

                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                    lastBubbleOutsideFOV = i;

                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}