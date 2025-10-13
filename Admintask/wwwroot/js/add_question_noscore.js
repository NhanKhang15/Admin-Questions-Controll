(function(){
    document.addEventListener('click', function (e) {
    // 1) Bấm "Thêm câu hỏi"
    const addBtn = e.target.closest('.aq-add-byset');
    if (addBtn) {
      const setId = addBtn.dataset.set;
      const row = document.getElementById('aq-new-row-' + setId);
      if (row) {
        row.style.display = 'table-row';
        // focus vào ô nhập nội dung
        const inp = row.querySelector('input[name="text"]');
        if (inp) {
          // delay 1 tick cho chắc visible rồi mới focus
          setTimeout(() => inp.focus(), 0);
        }
      }
      return; // chặn bubbling tiếp
    }

    // 2) Bấm "Hủy"
    const cancelBtn = e.target.closest('.aqBySetCancel');
    if (cancelBtn) {
      const setId = cancelBtn.dataset.set;
      const row = document.getElementById('aq-new-row-' + setId);
      if (row) {
        row.style.display = 'none';
        // reset form
        const form = document.getElementById('aqNewForm-' + setId);
        if (form) form.reset();
      }
      return;
    }
  });
})();