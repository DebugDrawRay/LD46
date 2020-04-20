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

		public GameObject m_pauseMenuUI;
		public Button m_pauseMenuFirstSelectedButton;

		[SerializeField]
		private string m_mainMenuSceneName;

		private Player m_input;

		private void Awake()
		{
			m_input = ReInput.players.GetPlayer(0);
			m_input.AddInputEventDelegate(Menu, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, Actions.Menu);
			
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

		void Pause()
		{
			m_pauseMenuUI.SetActive(true);
			Time.timeScale = 0f;
			GameController.Instance.CurrentGameState = GameController.GameState.Paused;


			// Select the button
			EventSystem.current.SetSelectedGameObject(m_pauseMenuFirstSelectedButton.gameObject);
			// Highlight the button
			m_pauseMenuFirstSelectedButton.OnSelect(new BaseEventData(EventSystem.current));
		}

		public void LoadMenu()
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(m_mainMenuSceneName);
		}

		public void QuitGame()
		{
			Debug.Log("Quiting game...");
			Application.Quit();
		}
	}

}