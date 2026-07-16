/*
Add this branch inside dbo._POS_ItemMaster before the final END.

The application uses this operation to preview the same next ItemNo that CREATE
will save. CREATE currently sets ItemNo = ItemId, so the preview must use the
next ItemId.
*/
ELSE IF(@_Operation = 'GETNEXTITEMNO')
BEGIN
    SELECT ISNULL(MAX(ItemId) + 1, 1) AS ItemNo
    FROM ItemMaster
END
