---
name: audit-trail
description: 添加审计功能指南。当需要添加AuditTrail审计跟踪功能时使用此技能。
---

# 将使用以下步骤对对应的模块添加审计跟踪功能

- 添加查看审计记录的按钮
- 在Service中添加审计记录逻辑
- 配置实体以支持审计跟踪

## 1. 找到对应的Edit Form并添加查看审计记录的按钮

使用 `@Html.ChangeHistoryBtn(Model)` 在编辑表单中添加查看审计记录的按钮。
例如 `\src\Plugins\ZKEACMS.Article\Views\Article\Edit.cshtml`

```
@using (Html.BeginForm())
{
    @Html.EditorForModel()
    <div class="toolBar">
        ...
        @Html.ChangeHistoryBtn(Model) <!-- 添加此行 -->
        <a class="btn btn-default" href="@Url.Action("Index")">@L("Cancel")</a>
    </div>
}
```

## 2. 找到对应的Service添加审计记录逻辑

添加注入`IAuditTrailService`，然后在Update方法中调用`AuditUpdate`方法记录变更。
例如 `\src\Plugins\ZKEACMS.Article\Services\ArticleService.cs`
```
public override ErrorOr<ArticleEntity> Update(ArticleEntity item)
{
    ...
    var oldItem = GetByID(item.ID);
    _auditTrailService.AuditUpdate(oldItem, item);
    ...
}
```

## 3. 配置实体

为需要审计的实体配置必要的属性，例如 `[AuditKey]`, `[AuditTitle]`, `[AuditIgnore]`。
如果继承自`EditorEntity`可不用添加 `[AuditTitle]`因为`EditorEntity`已有属性标记为Title。

### 1. 必需属性

- 仅对于集合元素，需要至少一个标记 `[AuditKey]` 属性的字段用于比较

### 2. 可选配置属性

- `[AuditTitle]`：指定用于显示的标题字段，可以有多个并按 Order 排序
- `[AuditIgnore]`：标记在类或属性上，表示在审计时忽略该类或属性
- `[AuditKey(Order = n)]`：指定复合键中的顺序


### 配置实体以支持审计跟踪

```csharp
public class ArticleEntity : EditorEntity, IImage
{
    [Key]
    public int ID { get; set; }
    public string Url { get; set; }
    public string Summary { get; set; }
    public string MetaKeyWords { get; set; }
    public string MetaDescription { get; set; }
    public int? Counter { get; set; }
    public string ArticleContent { get; set; }
    public string ImageThumbUrl { get; set; }
    public string ImageUrl { get; set; }
    public int? ArticleTypeID { get; set; }
    public DateTime? PublishDate { get; set; }
    public bool IsPublish { get; set; }
}
```