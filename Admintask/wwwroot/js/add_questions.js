(function () {
  const KEY = 'qa_scroll_anchor';

  // === CLICK HANDLER DUY NHẤT (delegation) ===
  document.addEventListener('click', function (e) {
    // 1) Add-by-set
    const addBtn = e.target.closest('.aq-add-byset');
    if (addBtn) {
      const setId = addBtn.getAttribute('data-set');
      // scored/non-score đều dùng form per-set: #aq-new-row-<id>
      const row = document.getElementById('aq-new-row-' + setId);
      if (row) {
        row.style.display = 'table-row';
        const inp = row.querySelector('input[name="text"]') || row.querySelector('input,textarea,select');
        setTimeout(() => inp && inp.focus(), 0);
      }
      return;
    }

    // 2) Cancel form theo set
    const cancelBtn = e.target.closest('.aqBySetCancel');
    if (cancelBtn) {
      const setId = cancelBtn.getAttribute('data-set');
      const row   = document.getElementById('aq-new-row-' + setId);
      if (row) {
        row.style.display = 'none';
        const form = document.getElementById('aqNewForm-' + setId);
        if (form) form.reset();
      }
      return;
    }

    // 3) Lưu anchor khi click link trong bảng (ví dụ "Sửa")
    const a = e.target.closest('a');
    if (a && a.closest('.aq-table-wrap')) {
      // Prefer an explicit tr[id] that matches our row/set id patterns.
      let anchorEl = a.closest('tr[id]');
      if (anchorEl) {
        const id = anchorEl.id || '';
        if (!/^row-q-|^set-/.test(id)) anchorEl = null;
      }

      // If we didn't find a clear row, try to find the nearest header row that has a valid id.
      if (!anchorEl) {
        const tr = a.closest('tr');
        const header = tr ? tr.previousElementSibling : null;
        if (header && header.id && /^set-/.test(header.id)) anchorEl = header;
      }

      // Save either the identified id, or fallback to current scroll Y if nothing suitable found.
      sessionStorage.setItem(KEY, anchorEl?.id || ('y:' + window.scrollY));
    }
  });

  // === Lưu anchor khi submit form trong bảng ===
  document.addEventListener('submit', function (e) {
    const form = e.target;
    if (!form.closest('.aq-table-wrap')) return;

    // Prefer explicit row/set id
    let anchorEl = form.closest('tr[id]');
    if (anchorEl) {
      const id = anchorEl.id || '';
      if (!/^row-q-|^set-/.test(id)) anchorEl = null;
    }

    if (!anchorEl) {
      const tr = form.closest('tr');
      const header = tr ? tr.previousElementSibling : null;
      if (header && header.id && /^set-/.test(header.id)) anchorEl = header;
    }
    sessionStorage.setItem(KEY, anchorEl?.id || ('y:' + window.scrollY));
  });

  // === Khôi phục vị trí sau reload ===
  document.addEventListener('DOMContentLoaded', function () {
    const saved = sessionStorage.getItem(KEY);
    if (!saved) return;

    if (saved.startsWith('y:')) {
      const y = parseInt(saved.slice(2), 10);
      if (!Number.isNaN(y)) window.scrollTo({ top: y });
    } else {
      const el = document.getElementById(saved);
      if (el) el.scrollIntoView({ block: 'center' });
    }
    sessionStorage.removeItem(KEY);
  });
})();