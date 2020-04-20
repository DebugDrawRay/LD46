using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Rewired;
using PBJ.Configuration.Input;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PBJ
{
	public class PauseMenuController : MonoBehaviour
	{
		public static PauseMenuController Instance;
		public GameObject m_pauseMenuUI;
		public GameObject m_gameOverUI;
		public Button m_pauseMenuFirstSelectedButton;
		public Button m_gameOverFirstSelectedButton;

		[SerializeField]
		private string m_mainMenuSceneName;
		[SerializeField]
		private string m_gameSceneName;
		[SerializeField]
		public Text m_gameOverHeader;
		[SerializeField]
		private Text m_happiness;
		[SerializeField]
		private Text m_eaten;
		[SerializeField]
		private Text m_thrown;
		[SerializeField]
		private Text m_destroy;
		[SerializeField]
		private Text m_attacked;
		[SerializeField]
		private Text m_time;

		private Player m_input;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(Instance.gameObject);
			}
			Instance = this;
			m_input = ReInput.players.GetPlayer(0);
			m_input.AddInputEventDelegate(Menu, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Actions.Menu);
			m_pauseMenuUI.SetActive(false);
			m_gameOverUI.SetActive(false);
		}

		private void Menu(InputActionEventData data)
		{
			if (GameController.Instance.CurrentGameState == GameController.GameState.Paused)
			{
				Resume();
			}
			else if (GameController.Instance.CurrentGameState == GameController.GameState.Playing)
			{
				Pause();
			}
		}

		public void Resume()
		{
			m_pauseMenuUI.SetActive(false);
			Time.timeScale = 1f;
			GameController.Instance.CurrentGameState = GameController.GameState.Playing;
		}

		public void EndScreen(bool gameOver, int eaten, int thrown, int destroyed, int attacked, float time)
		{
			m_gameOverUI.SetActive(true);
			Time.timeScale = 0f;
			GameController.Instance.CurrentGameState = GameController.GameState.Paused;

			// Select the button
			EventSystem.current.SetSelectedGameObject(m_gameOverFirstSelectedButton.gameObject);
			// Highlight the button
			m_pauseMenuFirstSelectedButton.OnSelect(new BaseEventData(EventSystem.current));

			m_gameOverHeader.text = gameOver ? "Game Over" : "The God Is Content";
			m_eaten.text = eaten.ToString() + " Items Eaten"; 
			m_thrown.text = thrown.ToString() + " Items Thrown"; 
			m_destroy.text = destroyed.ToString() + " Items Destroyed"; 
			m_attacked.text = attacked.ToString() + " People Attacked"; 
			m_time.text = "You Lasted: " + time.ToString() + " Seconds"; 
		}
		void Pause()
		{
			if (!m_gameOverUI.activeSelf)
			{
				m_pauseMenuUI.SetActive(true);
				Time.timeScale = 0f;
				GameController.Instance.CurrentGameState = GameController.GameState.Paused;


				// Select the button
				EventSystem.current.SetSelectedGameObject(m_pauseMenuFirstSelectedButton.gameObject);
				// Highlight the button
				m_pauseMenuFirstSelectedButton.OnSelect(new BaseEventData(EventSystem.current));
			}
		}

		public void LoadMenu()
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(m_mainMenuSceneName);
			if (GameController.Instance)
			{
				GameController.Instance.OnQuit();
			}
		}

		public void QuitGame()
		{
			Debug.Log("Quiting game...");
			Application.Quit();
		}
	}

}