namespace MapOptionGuard;

/// <summary>OCR이 읽은 한 줄.</summary>
/// <param name="Text">읽은 글자. 글머리표로 읽힌 글자는 뺀다.</param>
/// <param name="StartsWithBullet">줄 앞에 옵션 글머리표(빨간 ◆)가 있는지. 옵션은 글머리표가 있는 줄에서 시작한다.</param>
/// <param name="HasSpaceBelow">아래에 줄 하나가 더 보일 만큼 빈 공간이 있는지. 없으면 다음 줄이 창 아래로 잘려 있을 수 있다.</param>
internal sealed record OcrTextLine(string Text, bool StartsWithBullet = false, bool HasSpaceBelow = true);
