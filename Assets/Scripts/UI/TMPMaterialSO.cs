using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TMPMaterialSO", menuName = "Gameplay/UI/TMPMaterialSO")]
public class TMPMaterialSO : ScriptableObject
{
    public Material materialNormal;
    public Material materialHard;
    public Material materialSuperHard;
    public Material materialNightmare;

    [ValueDropdown(nameof(GetAllTerms))]
    public string materialNormalTerm;
    [ValueDropdown(nameof(GetAllTerms))]
    public string materialHardTerm;
    [ValueDropdown(nameof(GetAllTerms))]
    public string materialSuperHardTerm;
    [ValueDropdown(nameof(GetAllTerms))]
    public string materialNightmareTerm;

    public Color normal;
    public Color hard;
    public Color veryHard;
    public Color nightmare;

    public IEnumerable<string> GetAllTerms()
    {
        return LocalizationUtils.GetAllTerms();
    }
}
