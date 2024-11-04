namespace AlgoVisualizer;

public interface INavigationService
{
    void NavigateTo<T>() where T : class;
}