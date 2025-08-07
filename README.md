# README

This document outlines the Git branching strategy, branch naming conventions, commit message format, and important workflow guidelines to maintain a clean and efficient development process.

---

## 1. Git Branching Strategy

* **Primary branches**:

  * `main`: The production-ready code.
  * `develop`: The integration branch for features and fixes.

* **Supporting branches**:

  * `feature/xyz`: For developing new features.
  * `fix/xyz`: For bug fixes.
  * `refactor/xyz`: For code refactoring without changing functionality.
  * `docs/xyz`: For documentation updates (README, comments, etc.).
  * `test/xyz`: For adding or updating test cases.

**Workflow**:

1. **Do not** push directly to `main` or `develop`.
2. Always pull the latest `develop` before creating a new branch:

   ```bash
   git checkout develop
   git pull origin develop
   ```
3. Create and switch to your feature/fix/refactor/docs/test branch:

   ```bash
   git checkout -b feature/your-task-name
   ```
4. Commit your changes following the commit message format (see below).
5. Regularly merge updates from `develop` into your branch to keep it up to date:

   ```bash
   git fetch origin develop
   git merge origin/develop
   # resolve any conflicts if necessary
   ```
6. When ready, push your branch and open a Pull Request (PR) against `develop`.
7. After code review and approval, merge your PR into `develop`.
8. For release, merge `develop` into `main` (typically by a release manager) after final testing.

---

## 2. Commit Message Format

Use a structured, consistent commit message format. Include:

1. **Summary line**: A short sentence in English (e.g., "Hoang has done task").
2. **Description body** (bullet list): A brief summary of changes.
3. **Type tag**: One of `[Feature]`, `[Fix]`, `[Refactor]`, `[Docs]`, `[Test]`.

### Template

<pre><code>[Type] Short description in Vietnamese or English

- Detail 1
- Detail 2
</code></pre>

### Types

* **Feature**: New functionality.
* **Fix**: Bug fixes.
* **Refactor**: Code restructuring without changing behavior.
* **Docs**: Documentation only changes (README, comments).
* **Test**: Adding or updating tests.

### Examples

```bash
git commit -m "[Feature] Thêm chức năng tìm kiếm người dùng"
# - Implement user search API
# - Add UI input and results list
```

```bash
git commit -m "[Fix] Sửa lỗi đăng nhập không hiển thị thông báo"
# - Handle null token response
# - Update error message text
```

```bash
git commit -m "[Refactor] Tách module xử lý email"
# - Extract email helper class
# - Update tests to use new class
```

```bash
git commit -m "[Docs] Cập nhật README hướng dẫn cài đặt"
# - Add environment variables section
# - Fix broken links
```

```bash
git commit -m "[Test] Thêm test case cho đăng ký tài khoản"
# - Cover valid and invalid inputs
# - Mock database calls
```

---

## 3. Important Notes

* **Branch updates**: Always merge the latest `develop` into your branch before finalizing.
* **Conflict resolution**: Resolve merge conflicts promptly and clearly.
* **PR reviews**: Require at least one code review before merging into `develop`.
* **Release process**: Only merge `develop` into `main` after thorough testing.
* **Naming consistency**: Keep branch and commit names descriptive and consistent.
