public interface IScrollControlDataSource
{
    int GetItemCount();
    void SetCell(ICell2 cell, int index, int columnIndex);
}
