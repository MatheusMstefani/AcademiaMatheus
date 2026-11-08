// Matheus Marques Stefani
using AcademiaDoZe.Application.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public sealed class RestricaoSelectable(AppMatriculaRestricoes value) : ObservableObject
{
    public AppMatriculaRestricoes Value { get; } = value;
    public string Nome => Value.GetDisplayName();
    private bool isSelected;
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
}
