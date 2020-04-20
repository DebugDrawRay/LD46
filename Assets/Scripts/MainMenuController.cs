using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PBJ
{
	public class MainMenuController : MonoBehaviour
	{
		public string m_gameSceneString;

		public GameObject m_mainMenuObject;
		public GameObject m_tutorialMenuObject;
		public Button m_tutorialButton;

		private void Awake()
		{
			Assert.IsNotNull(m_gameSceneString);
			Assert.IsNotNull(m_mainMenuObject);
			Assert.IsNotNull(m_tutorialMenuObject);
			Assert.IsNotNull(m_tutorialButton);

			m_mainMenuObject.SetActive(true);
			m_tutorialMenuObject.SetActive(false);
		}

		public void LoadScene()
		{
			Debug.Log("Playing game...");
			SceneManager.LoadScene(m_gameSceneString);
		}

		public void PlayGame()
		{
			Debug.Log("Showing tutorial screen..");
			m_mainMenuObject.SetActive(false);
			m_tutorialMenuObject.SetActive(true);

			// Select the button
			EventSystem.current.SetSelectedGameObject(m_tutorialButton.gameObject);
			// Highlight the button
			m_tutorialButton.OnSelect(new BaseEventData(EventSystem.current));
		}

		public void QuitGame()
		{
			Debug.Log("Quiting game...");
			Application.Quit();
		}
	}
}
