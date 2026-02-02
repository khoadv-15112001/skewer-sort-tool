using I2.Loc;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIMultiEventLoseBase : MonoBehaviour, IMultiEventLose
{
    [SerializeField] protected UIMultiEventLoseController.EVerbType verbType;

    [ValueDropdown(nameof(GetAllTerms))]
    [SerializeField] protected string term;

    public virtual bool Setup()
    {
        return false;
    }

    public virtual string GetName()
    {
        return LocalizationManager.GetTranslation(term);
    }

    public virtual UIMultiEventLoseController.EVerbType GetVerbType()
    {
        return verbType;
    }

    public static IEnumerable<string> GetAllTerms()
    {
        return LocalizationUtils.GetAllTerms();
    }

}
