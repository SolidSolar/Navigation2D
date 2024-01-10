using System;
using UnityEngine;

namespace DialogUtilitySpruce.API
{
    public static class DialogReaderSettings
    {
        public const string DefaultLanguage = "English";
        public static string Language { get; private set; }
        public static Action<string> OnInitialize { get; set; }

        private const string CharacterLocalisationPath = "DialogUtility/CharacterLocalisation/{0}";
        private const string LocalisationPath = "DialogUtility/ContainerLocalisation/{0}/{1}";

        public static void Initialize(string language)
        {
            Language = language;
            CharacterLocalisation = Resources.Load<LocalisationResource>(string.Format(CharacterLocalisationPath, language));
            OnInitialize?.Invoke(Language);
        }

        public static LocalisationResource GetContainerLocalisation(string containerName)
        {
            return Resources.Load<LocalisationResource>(string.Format(LocalisationPath, Language, containerName));
        }
        
        public static LocalisationResource CharacterLocalisation;
    }
}