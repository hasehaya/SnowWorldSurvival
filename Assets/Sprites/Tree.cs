using CryingSnow.FastFoodRush;

using UnityEngine;

public class Tree :MonoBehaviour
{
    [SerializeField] private int treeHealth = 5;      // �c���[�̏����w���X
    [SerializeField] private int logCount = 3;        // �w���X��1���邲�Ƃɐ������郍�O��
    [SerializeField] private float decreaseInterval = 3f; // �w���X������Ԋu(�b)
    [SerializeField] private float upwardForce = 5f;  // ���O����ɒ��ˏグ���

    private float timer = 0f;

    private void OnTriggerStay(Collider other)
    {
        // �v���C���[���g���K�[���ɂ���ꍇ�̂ݏ���
        if (other.CompareTag("Player"))
        {
            // �g���K�[���ɂ���ԁA�o�ߎ��Ԃ����Z
            timer += Time.deltaTime;

            // ��莞�Ԃ𒴂�����w���X�����炵�A���O����
            if (timer >= decreaseInterval)
            {
                // �w���X��1���炷
                treeHealth--;

                // ���O�𐶐�
                SpawnLogs();

                // �w���X���Ȃ��Ȃ�����؂�j��
                if (treeHealth <= 0)
                {
                    Destroy(gameObject);
                }

                // �^�C�}�[�����Z�b�g
                timer = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // �v���C���[���g���K�[����o����^�C�}�[�����Z�b�g(�G��Ă��Ȃ��Ԃ͌���Ȃ��悤�ɂ���)
        if (other.CompareTag("Player"))
        {
            timer = 0f;
        }
    }

    /// <summary>
    /// logCount ���̃��O�𐶐����āA��ɒ��ˏグ��
    /// </summary>
    private void SpawnLogs()
    {
        for (int i = 0; i < logCount; i++)
        {
            // PoolManager ���g���Ă���ꍇ�̗� (�Ȃ���� Instantiate �ł�OK)
            var log = PoolManager.Instance.SpawnObject("Log");

            // ���������������炵�Đ���
            log.transform.position = transform.position + Vector3.up * (0.5f * i);

            var rb = log.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // �v�[�����ꂽ�I�u�W�F�N�g�̂��߁A�O�̂��ߑ��x�����Z�b�g
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // ������ɏu�ԓI�ȗ͂������� => ���˂Ă��̌㗎��
                rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
            }
        }
    }
}
